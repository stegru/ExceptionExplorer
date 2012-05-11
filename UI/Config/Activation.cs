namespace ExceptionExplorer.Config
{
    using System;
    using System.Text;
    using System.Security.Cryptography;
    using System.Xml;
    using System.Xml.Serialization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using ExceptionExplorer.General;
    using System.Windows.Forms;
    using ExceptionExplorer.UI;

    public static class ActivationExtensions
    {
        /// <summary>Gets the base64 encoding of some bytes.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The base64 encoding of the data.</returns>
        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Gets the base64 encoding of some bytes.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// The base64 encoding of the data.
        /// </returns>
        public static string ToBase64(this byte[] data, int length)
        {
            return Convert.ToBase64String(data, 0, length);
        }

        /// <summary>Hashes the specified string.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The SHA256 hash of the string.</returns>
        public static string Hash(this string str)
        {
            return Encoding.UTF8.GetBytes(str).Hash();
        }

        /// <summary>Hashes the specified data.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The SHA256 hash of the data.</returns>
        public static string Hash(this byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(data).ToBase64(32);
            }
        }
    }

    [XmlRoot(ElementName = "ExceptionExplorerActivation")]
    public class Licence : IStorable
    {
        /// <summary>The class version. Incremented when this class changes.</summary>
        protected const int ClassVersion = 1;

        protected const string Base64Begin = "-----Begin Exception Explorer Activation Code-----";
        protected const string Base64End = "-----End Exception Explorer Activation Code-----";

        /// <summary>Gets or sets the class version that the licence file was created with. .</summary>
        /// <value>The details version.</value>
        [XmlAttribute(AttributeName = "version")]
        public int DetailsVersion { get; set; }

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>Gets or sets the id.</summary>
        /// <value>The id.</value>
        [XmlElement(ElementName = "OrderId")]
        public string Id { get; set; }

        /// <summary>Gets or sets the valid from.</summary>
        /// <value>The valid from.</value>
        [XmlElement(DataType = "date", ElementName = "Date")]
        public DateTime ValidFrom { get; set; }

        /// <summary>Gets or sets the valid to.</summary>
        /// <value>The valid to.</value>
        [XmlElement(ElementName = "Expires", IsNullable = true)]
        public string Expires
        {
            get
            {
                if (this.ValidTo.Date == DateTime.MaxValue.Date)
                {
                    return null;
                }
                else
                {
                    return XmlConvert.ToString(this.ValidTo, "yyyy-MM-dd");
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.ValidTo = DateTime.MaxValue.Date;
                }
                else
                {
                    this.ValidTo = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
                }
            }
        }

        public bool ShouldSerializeExpires()
        {
            return !(this.ValidTo.Date == DateTime.MaxValue.Date || this.ValidTo.Date == DateTime.MinValue.Date);
        }

        [XmlIgnore]
        public DateTime ValidTo { get; set; }

        /// <summary>Gets or sets the current latest version, at the time of signing.</summary>
        /// <value>The version.</value>
        public string Version { get; set; }

        /// <summary>Gets or sets the signature.</summary>
        /// <value>The signature.</value>
        public virtual string Signature
        {
            get
            {
                return this.ActualSignature;
            }
            set
            {
                if (value == null)
                {
                    this.ActualSignature = null;
                }
                else
                {
                    string sig = value;
                    // remove all whitespace
                    StringBuilder sb = new StringBuilder(sig.Length);
                    foreach (char ch in sig)
                    {
                        switch (ch)
                        {
                            case '\r':
                            case '\n':
                            case '\t':
                            case ' ':
                                continue;
                            default:
                                sb.Append(ch);
                                break;
                        }
                    }

                    this.ActualSignature = sb.ToString();
                    this.RawSignature = Convert.FromBase64String(this.ActualSignature);
                }
            }
        }

        /// <summary>The salt, used when encrypting.</summary>
        [XmlIgnore]
        private byte[] salt;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Licence"/> is loaded correctly.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loaded; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        internal bool Loaded { get; private set; }

        internal enum LicenceStatus
        {
            Invalid = 0,
            OK = 1,
            Expired = 2,
        }

        [XmlIgnore]
        internal LicenceStatus Status { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return (bool)(this._isValid ?? (this._isValid = this.Loaded && this.Verify()));
            }
        }

        /// <summary>Is this licence valid.</summary>
        private bool? _isValid;

        /// <summary>
        /// Gets or sets the actual signature.
        /// </summary>
        /// <value>The actual signature.</value>
        [XmlIgnore]
        protected internal string ActualSignature { get; protected set; }

        /// <summary>Gets or sets the raw signature.</summary>
        /// <value>The raw signature.</value>
        [XmlIgnore]
        protected byte[] RawSignature { get; set; }

        /// <summary>Gets the entropy for the encryption.</summary>
        [XmlIgnore]
        private byte[] entropy
        {
            get
            {
                if ((salt == null) || (salt.Length == 0))
                {
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        this.salt = new byte[20];
                        rng.GetNonZeroBytes(this.salt);
                    }
                }

                return this.salt;
            }
        }

        /// <summary>Encodes the specified data.</summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] Encode(byte[] data)
        {
            return ProtectedData.Protect(data, this.entropy, DataProtectionScope.LocalMachine);
        }

        /// <summary>
        /// Decodes the specified cypher text.
        /// </summary>
        /// <param name="cypherText">The cypher text.</param>
        /// <returns></returns>
        private byte[] Decode(byte[] cypherText)
        {
            return ProtectedData.Unprotect(cypherText, this.entropy, DataProtectionScope.LocalMachine);
        }

        /// <summary>Encodes the signature.</summary>
        /// <returns></returns>
        internal byte[] EncodeSignature()
        {
            return this.Encode(this.RawSignature);
        }

        /// <summary>Decodes the signature.</summary>
        /// <param name="encodedSignature">The encoded signature.</param>
        internal void DecodeSignature(byte[] encodedSignature)
        {
            byte[] plaintext = this.Decode(encodedSignature);
            this.RawSignature = plaintext;
            this.ActualSignature = plaintext.ToBase64();
        }

        private static byte[] exponent = new byte[] { 0x01, 0x00, 0x01 };

        private static byte[] modulus = new byte[] {
            0xC2, 0x90, 0xE0, 0xB2, 0xE8, 0x19, 0xEE, 0xD6, 0x9E, 0x75, 0x80, 0x6A, 0xD6, 0x59, 0xCF, 0x17, 0xBB, 0xE8, 0xB5,
            0x58, 0xAA, 0x78, 0x7E, 0x51, 0x2A, 0xF4, 0x1A, 0x6B, 0x0C, 0x5B, 0xCA, 0x4A, 0xE4, 0xA9, 0xC7, 0x9D, 0x1C, 0x3E,
            0x81, 0x8E, 0xE5, 0x94, 0x6D, 0x57, 0xBD, 0xE1, 0x5B, 0x99, 0x8E, 0x81, 0x43, 0x1E, 0x29, 0x36, 0x86, 0xF7, 0xE9,
            0xBF, 0x33, 0x8A, 0x3B, 0x87, 0x1D, 0xB6, 0x98, 0xBA, 0x09, 0x98, 0x59, 0x2D, 0x36, 0x59, 0x21, 0x98, 0xA5, 0xD9,
            0xF4, 0x08, 0xFF, 0xB0, 0x87, 0xDF, 0xB4, 0x62, 0x96, 0x21, 0x3C, 0xBD, 0xE2, 0x64, 0xC1, 0x80, 0x1A, 0x93, 0x1D,
            0xAC, 0x02, 0xC0, 0x89, 0xA4, 0x1B, 0xA0, 0xD1, 0x75, 0x53, 0x7F, 0x57, 0xF9, 0xB9, 0xA3, 0x74, 0x06, 0x3F, 0x11,
            0x2C, 0x79, 0xDA, 0x9F, 0x57, 0x58, 0x06, 0x5B, 0x01, 0x17, 0x14, 0xB3, 0x16, 0xE3
        };

        public static RSA PublicKey()
        {
            RSA key = RSA.Create();

            RSAParameters p = new RSAParameters();
            p.Modulus = modulus;
            p.Exponent = exponent;

            key.ImportParameters(p);
            return key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Licence"/> class.
        /// </summary>
        public Licence()
        {
            this.ValidTo = DateTime.MaxValue;
            this.Status = LicenceStatus.Invalid;
        }

        /// <summary>Gets the string used for signing.</summary>
        /// <returns>The signing data.</returns>
        public string GetSigningData()
        {
            return string.Format(
                "{0}#{1}#{2}#{3:yyyyMMdd}#{4:yyyyMMdd}#{5}#",
                this.Name,
                this.Email,
                this.Id,
                this.ValidFrom,
                this.ValidTo,
                this.Version
                ).ToLowerInvariant();
        }

        /// <summary>Gets the details element from the XML document.</summary>
        /// <param name="doc">The xml document.</param>
        /// <returns>The details element.</returns>
        private static XmlElement GetDetailsElement(XmlDocument doc)
        {
            return doc.DocumentElement;
        }

        /// <summary>Verifies this instance, check the signature.</summary>
        /// <returns>true if it is valid.</returns>
        internal bool Verify()
        {
            string message = this.GetSigningData();
            bool valid = false;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                using (RSA key = PublicKey())
                {
                    rsa.ImportParameters(key.ExportParameters(false));

                    SHA512Managed hash = new SHA512Managed();
                    byte[] hashBytes = hash.ComputeHash(data);

                    valid = rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA512"), Convert.FromBase64String(this.ActualSignature));
                }
            }

            return valid;
        }

        private bool OnLoad()
        {
            this.Loaded = true;

            if (!this.Verify())
            {
                return false;
            }

            // see if it's expired
            if (this.ValidTo.Date < DateTime.Now.Date)
            {
                this.Status = LicenceStatus.Expired;
            }
            else
            {
                this.Status = LicenceStatus.OK;
            }

            return true;
        }

        /// <summary>Loads the licence information from a string.</summary>
        /// <param name="path">The path.</param>
        /// <returns>CustomerDetails instance.</returns>
        internal static Licence LoadFromXML(string xmlText)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            return Load(doc);
        }

        /// <summary>Loads the licence information from a file.</summary>
        /// <param name="path">The path.</param>
        /// <returns>CustomerDetails instance.</returns>
        internal static Licence LoadFromFile(string path)
        {
            string base64 = File.ReadAllText(path);
            return LoadFromBase64(base64);
        }

        public static Licence LoadFromBase64(string text)
        {
            string base64 = null;

            Regex re = new Regex(
                @"-+\s*begin.*-(?<base64>([a-z0-9/+]|\s)+=*)\s*-+\s*end.*-+",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            Match match = re.Match(text);

            if (match.Success && match.Groups["base64"].Success)
            {
                base64 = match.Groups["base64"].Value.Trim();
            }
            else
            {
                Regex base64Regex = new Regex(@"^(?<base64>([a-z0-9/+]|\s)+=*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                match = base64Regex.Match(text);
                if (match.Success && match.Groups["base64"].Success)
                {
                    base64 = match.Groups["base64"].Value.Trim();
                }
            }

            if (base64 == null)
            {
                return null;
            }

            byte[] data;
            try
            {
                data = Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return null;
            }

            return LoadFromXML(Encoding.UTF8.GetString(data));
        }

        /// <summary>Loads the specified licence xml document.</summary>
        /// <param name="doc">The doc.</param>
        /// <returns>The Licence.</returns>
        private static Licence Load(XmlDocument doc)
        {
            XmlElement details = GetDetailsElement(doc);

            using (StringReader input = new StringReader(details.OuterXml))
            {
                XmlSerializer s = new XmlSerializer(typeof(Licence));
                Licence lic = s.Deserialize(input) as Licence;

                bool valid = lic.OnLoad();

                if (!valid)
                {
                    throw new ActivationException();
                }

                return lic;
            }
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Save(Storage store)
        {
            if (this.IsValid)
            {
                store.SetValue("Name", this.Name);
                store.SetValue("Email", this.Email);
                store.SetValue("ValidFrom", this.ValidFrom.ToBinary());
                if (this.ShouldSerializeExpires())
                {
                    store.SetValue("ValidTo", this.ValidTo.ToBinary());
                }
                store.SetValue("Version", this.Version);
                store.SetValue("OrderId", this.Encode(Encoding.UTF8.GetBytes(this.Id ?? string.Empty)));
                store.SetValue("Signature", this.EncodeSignature());
                store.SetValue("Salt", this.entropy);
            }
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Load(Storage store)
        {
            // get the name
            this.Name = store.GetValue("Name", string.Empty);

            if (string.IsNullOrEmpty(this.Name))
            {
                return;
            }

            // get the email
            this.Email = store.GetValue("Email", string.Empty);

            if (string.IsNullOrEmpty(this.Email))
            {
                return;
            }

            // date the valid from/to dates
            long from = store.GetValue("ValidFrom", (long)0);
            if (from == 0)
            {
                return;
            }

            long to = store.GetValue("ValidTo", (long)0);
            if (to == 0)
            {
                return;
            }

            try
            {
                this.ValidFrom = DateTime.FromBinary(from);
                this.ValidTo = to == 0 ? DateTime.MaxValue : DateTime.FromBinary(to);
            }
            catch (ArgumentException)
            {
                // dates are out of range
                return;
            }

            // this is currently not checked
            this.Version = store.GetValue("Version", string.Empty);

            // get the salt (this must be done before the encoded values are retrieved)
            this.salt = store.GetValue("Salt", new byte[] { });

            if (this.salt.Length == 0)
            {
                return;
            }

            // get the encoded order id
            byte[] encodedId = store.GetValue("OrderId", new byte[] { });

            if (encodedId.Length == 0)
            {
                return;
            }

            // decode
            this.Id = Encoding.UTF8.GetString(this.Decode(encodedId));

            // get the encoded signature
            byte[] encodedSig = store.GetValue("Signature", new byte[] { });

            if (encodedSig.Length == 0)
            {
                return;
            }

            this.DecodeSignature(encodedSig);

            // everything was successfully loaded (but not necessarily valid)
            this.OnLoad();
        }

        /// <summary>
        /// Info about the usage, used when not licenced.
        /// </summary>
        public struct Info
        {
            /// <summary>The day the application was installed (first run).</summary>
            public int InstallDay;

            /// <summary>The day the application was last run.</summary>
            public int RunDay;

            /// <summary>Number of times the application has run for.</summary>
            public int RunCount;

            /// <summary>Number of distinct days the application was run in.</summary>
            public int DayCount;

            /// <summary>The current level, as a byte.</summary>
            private byte CurrentLevelByte;

            /// <summary>The current level</summary>
            private TrialLevel CurrentLevel
            {
                get
                {
                    return (TrialLevel)this.CurrentLevelByte;
                }
                set
                {
                    this.CurrentLevelByte = Convert.ToByte((int)this.CurrentLevelByte);
                }
            }

            /// <summary>Gets the number of days installed.</summary>
            public int DaysInstalled
            {
                get
                {
                    return Today - this.InstallDay;
                }
            }

            /// <summary>The epoch used for the day counters</summary>
            private static DateTime Epoch = new DateTime(2012, 01, 01);

            /// <summary>Gets the day number of the specified date.</summary>
            /// <param name="date">The date.</param>
            /// <returns>The number of days since the epoch.</returns>
            private static short Days(DateTime date)
            {
                return (short)Math.Max(0, DateTime.Now.Subtract(Epoch).Days);
            }

            /// <summary>Gets the current day number.</summary>
            private static short Today
            {
                get
                {
                    return Days(DateTime.Now);
                }
            }

            /// <summary>Creates a new instance of this structure.</summary>
            /// <returns>A new initialised Info struct.</returns>
            private static Info CreateNew()
            {
                return new Info()
                {
                    DayCount = 1,
                    RunCount = 1,
                    InstallDay = Today,
                    RunDay = Today
                };
            }

            private const int SizeOf = 4 * sizeof(short) + sizeof(byte);

            /// <summary>Initialises a new Info struct froms the specified bytes.</summary>
            /// <param name="data">The data.</param>
            /// <returns>An Info structure.</returns>
            public static Info FromBytes(byte[] data)
            {
                if (data == null || data.Length < SizeOf)
                {
                    return CreateNew();
                }

                Info info = new Info();

                using (BinaryReader reader = new BinaryReader(new MemoryStream(data, false)))
                {
                    info.InstallDay = reader.ReadInt16();
                    info.RunDay = reader.ReadInt16();
                    info.RunCount = reader.ReadInt16();
                    info.DayCount = reader.ReadInt16();
                    info.CurrentLevelByte = reader.ReadByte();
                }

                info.CurrentLevel = (TrialLevel)info.CurrentLevelByte;

                info.RunCount++;

                if (info.RunDay != Today)
                {
                    info.DayCount++;
                }

                info.Level = info.GetLevel();

                return info;
            }

            /// <summary>Converts this structure into bytes.</summary>
            /// <returns>An array of bytes, representing the values from this struct.</returns>
            public byte[] ToBytes()
            {
                byte[] data = new byte[SizeOf];
                using (MemoryStream ms = new MemoryStream(data, true))
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)this.InstallDay);
                        writer.Write((short)Today);
                        writer.Write((short)this.RunCount);
                        writer.Write((short)this.DayCount);
                        writer.Write((byte)((int)this.Level & 0xff));
                    }
                }

                return data;
            }

            /// <summary>Gets or sets the current level.</summary>
            /// <value>The level.</value>
            public TrialLevel Level { get; private set; }

            public bool LevelChanged
            {
                get
                {
                    return this.Level != this.CurrentLevel;
                }
            }

            /// <summary>Gets the trial warning level.</summary>
            /// <returns></returns>
            private TrialLevel GetLevel()
            {
                int counter = Math.Max(this.RunCount, this.DayCount);
                int daysInstalled = this.DaysInstalled;

                if ((counter > 30) && (daysInstalled > 30))
                {
                    return TrialLevel.Stop;
                }

                if ((counter > 20) || ((daysInstalled > 45) && (counter > 5)))
                {
                    return TrialLevel.Warn;
                }

                if ((counter > 10) || (daysInstalled > 20))
                {
                    return TrialLevel.Remind;
                }

                return TrialLevel.OK;
            }
        }

        [XmlIgnore]
        public Info TrialInfo { get; private set; }

        public TrialLevel TrialLevel
        {
            get { return this.TrialInfo.Level; }
        }

        internal void RecordUsage()
        {
            this.TrialInfo = Info.FromBytes(Options.Current.InstallInfo.Value);
            Options.Current.InstallInfo.Value = this.TrialInfo.ToBytes();
        }
    }

    public enum TrialLevel
    {
        OK = 0,

        /// <summary>10 runs or 20 days</summary>
        Remind = 1,

        /// <summary>20 runs or 45 days</summary>
        Warn = 2,

        /// <summary>30 runs and 30 days</summary>
        Stop = 3,

        Grace = 4,
    }

    /// <summary>
    ///
    /// </summary>
    internal static class Activation
    {
        public static string Fix(this string text)
        {
            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                return text.Replace("icence", "icense");
            }

            return text;
        }

        public static bool CheckLicence(Licence licence)
        {
            if (licence == null)
            {
                return false;
            }

            if (licence.Status != Licence.LicenceStatus.OK)
            {
                // trial
                licence.RecordUsage();



                return false;
            }

            return true;
        }

        public static void LicenceMessage(IWin32Window owner)
        {
            Licence lic = Options.Current.Licence;

            if (lic.IsValid)
            {
                return;
            }

            string caption = null;
            string message = null;
            bool die = false;

            if (lic.TrialInfo.LevelChanged)
            {
                switch (lic.TrialLevel)
                {
                    case TrialLevel.Remind:
                        caption = "Reminder";
                        message = "Don't forget, this is the trial version of Exception Explorer - but it wont last forever.\n\nConsider purchasing a licence if you would like to continue using Exception Explorer.";
                        break;
                    case TrialLevel.Warn:
                        caption = "Your trial will end soon!";
                        message = "Your time to use Exception Explorer will soon run out, so purchase a licence this week to avoid disappointment!";
                        break;
                    case TrialLevel.Stop:
                        caption = "Final chance...";
                        message = "This is your final chance to try out Exception Explorer.\n\nAfter this session, you will no longer be able to use this software without purchasing a licence.";
                        break;
                }
            }
            else if (lic.TrialLevel == TrialLevel.Stop)
            {
                caption = "Purchase a licence today.";
                message = "Unfortunately, your time to try Exception Explorer has now come to an end.\n\nFor this to happen, you must have been using this software enough to find it useful...\n\nFeel free to purchace a licence if you would like to continue using Exception Explorer.";
                die = true;
            }

            if (message != null)
            {
                TaskDialog td = new TaskDialog()
                {
                    InstructionText = caption.Fix(),
                    Text = message.Fix(),
                    StandardButtons = TaskDialogStandardButtons.Close,
                    Cancelable = true,
                    DetailsExpandedText = string.Format("Installed for {0} days.\nRan {1} times, over {2} distinct days.\n", lic.TrialInfo.DaysInstalled, lic.TrialInfo.RunCount, lic.TrialInfo.DayCount).Fix(),
                };

                TaskDialogCommandLink purchase = new TaskDialogCommandLink("purchase", "Purchase Licence".Fix());
                purchase.Click += (sender, e) =>
                {
                    Web.OpenSite(SiteLink.Buy);
                    td.Close();
                };

                td.Controls.Add(purchase);

                if (!die)
                {
                    TaskDialogCommandLink later = new TaskDialogCommandLink("later", "Maybe later");
                    later.Click += (sender, e) =>
                    {
                        td.Close();
                    };
                }

                td.Show(owner);

                if (die)
                {
                    Application.Exit();
                }
            }
        }


        public static Licence LoadLicenceFromString(string text)
        {
            return LoadLicence(null, text);
        }

        public static Licence LoadLicenceFromFile(string path)
        {
            return LoadLicence(path, null);
        }

        private static Licence LoadLicence(string path, string text)
        {
            Licence c;
            if (path != null)
            {
                c = Licence.LoadFromFile(path);
            }
            else
            {
                c = Licence.LoadFromBase64(text);
            }

            return c ?? new Licence();
        }

        public static bool Activate(Licence licence)
        {
            if (!licence.IsValid)
            {
                return false;
            }

            Options.Current.Licence = licence;
            Options.Current.Save();

            return true;
        }
    }

    /// <summary>
    /// Activation Exception.
    /// </summary>
    [Serializable]
    public class ActivationException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ActivationException"/> class.</summary>
        public ActivationException() { }

        /// <summary>Initializes a new instance of the <see cref="ActivationException"/> class.</summary>
        /// <param name="message">The message.</param>
        public ActivationException(string message) : base(message) { }

        /// <summary>Initializes a new instance of the <see cref="ActivationException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ActivationException(string message, Exception inner) : base(message, inner) { }

        /// <summary>Initializes a new instance of the <see cref="ActivationException"/> class.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        ///
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected ActivationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}