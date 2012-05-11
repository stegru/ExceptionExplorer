namespace ExceptionExplorer.Config
{
    using System;
    using Microsoft.Win32;
    using System.IO;

    /// <summary>
    /// Identifies something that can be loaded and saved.
    /// </summary>
    public interface IStorable
    {
        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        void Save(Storage store);

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        void Load(Storage store);
    }

    public interface IStoreLocation
    {
    }

    /// <summary>
    /// A storage location for settings
    /// </summary>
    public abstract class Storage : IDisposable
    {
        public virtual IStoreLocation Location { get; protected set; }

        /// <summary>Creates a windows registry storage.</summary>
        /// <param name="keyname">The keyname.</param>
        /// <param name="write">if set to <c>true</c> [write].</param>
        /// <returns></returns>
        public static RegistryStore RegistryStore(string keyname, bool write)
        {
            return new RegistryStore(Path.Combine("software", keyname), write);
        }

        /// <summary>Opens storage, using the location.</summary>
        /// <param name="location">The location.</param>
        /// <returns>A Storage.</returns>
        public static Storage OpenLocation(IStoreLocation location, bool write)
        {
            Type storeType = location.GetType().DeclaringType;
            if (storeType == null)
            {
                throw new ArgumentException("Unknown IStoreLocation", "location");
            }

            return Activator.CreateInstance(storeType, location, write) as Storage;
        }

        /// <summary>Gets all value names.</summary>
        /// <returns>An array of value names.</returns>
        public abstract string[] GetAllValueNames();

        /// <summary>Gets the value, as an object.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as an object.</returns>
        public abstract object GetObjectValue(string name, object defaultValue);

        /// <summary>Gets a string value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a string.</returns>
        public abstract string GetValue(string name, string defaultValue);

        /// <summary>Gets a numeric value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as an int.</returns>
        public abstract int GetValue(string name, int defaultValue);

        /// <summary>Gets a numeric value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a long int.</returns>
        public abstract long GetValue(string name, long defaultValue);

        /// <summary>Gets a boolean value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a boolean.</returns>
        public abstract bool GetValue(string name, bool defaultValue);

        /// <summary>Gets a byte array value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as an array of bytes.</returns>
        public abstract byte[] GetValue(string name, byte[] defaultValue);

        /// <summary>Gets the value, of enumeration by TEnum.</summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public abstract TEnum GetValue<TEnum>(string name, TEnum defaultValue) where TEnum : struct;

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(string name, string value);

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(string name, int value);

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(string name, long value);

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(string name, bool value);

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(string name, byte[] value);

        /// <summary>Sets the value.</summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue<TEnum>(string name, TEnum value) where TEnum : struct;

        /// <summary>Creates a sub-store.</summary>
        /// <param name="name">The name.</param>
        /// <returns>A storage.</returns>
        public abstract Storage CreateSubStore(string name);

        /// <summary>Releases unmanaged and - optionally - managed resources</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class DummyStore : Storage
    {
        public override object GetObjectValue(string name, object defaultValue)
        {
            return defaultValue;
        }

        public override string GetValue(string name, string defaultValue)
        {
            return defaultValue;
        }

        public override int GetValue(string name, int defaultValue)
        {
            return defaultValue;
        }

        public override long GetValue(string name, long defaultValue)
        {
            return defaultValue;
        }

        public override bool GetValue(string name, bool defaultValue)
        {
            return defaultValue;
        }

        public override byte[] GetValue(string name, byte[] defaultValue)
        {
            return defaultValue;
        }

        public override TEnum GetValue<TEnum>(string name, TEnum defaultValue)
        {
            return defaultValue;
        }

        public override void SetValue(string name, string value)
        {
        }

        public override void SetValue(string name, int value)
        {
        }

        public override void SetValue(string name, long value)
        {
        }

        public override void SetValue(string name, bool value)
        {
        }

        public override void SetValue(string name, byte[] value)
        {
        }

        public override void SetValue<TEnum>(string name, TEnum value)
        {
        }

        public override Storage CreateSubStore(string name)
        {
            return new DummyStore();
        }

        public override string[] GetAllValueNames()
        {
            return new string[0];
        }
    }

    public class RegistryStore : Storage
    {
        public class RegistryStoreLocation : IStoreLocation
        {
            internal string KeyName { get; private set; }

            public RegistryStoreLocation(string keyName)
            {
                this.KeyName = keyName;
            }
        }

        private RegistryKey key;
        private bool canWrite;

        /// <summary>Gets a value indicating whether this instance is valid.</summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        private bool isValid
        {
            get { return this.key != null; }
        }

        /// <summary>Initializes a new instance of the <see cref="RegistryStore"/> class.</summary>
        /// <param name="keyname">The keyname.</param>
        /// <param name="write">Set to <c>true</c> for write access.</param>
        public RegistryStore(string keyname, bool write)
            : this(null, keyname, write)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RegistryStore"/> class.</summary>
        /// <param name="location">The location.</param>
        /// <param name="write">if set to <c>true</c> [write].</param>
        public RegistryStore(RegistryStoreLocation location, bool write)
            : this(location.KeyName, write)
        {
            this.Location = location;
        }

        public override void Dispose(bool disposing)
        {
            if (this.key != null)
            {
                this.key.Close();
                this.key.Dispose();
            }

            base.Dispose(disposing);
        }

        private RegistryStore()
        {
            this.key = null;
            this.canWrite = false;
            this.Location = new RegistryStoreLocation(null);
        }

        /// <summary>Prevents a default instance of the <see cref="RegistryStore"/> class from being created.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="keyname">The keyname.</param>
        /// <param name="write">Set to <c>true</c> for write access.</param>
        private RegistryStore(RegistryKey parent, string keyname, bool write)
        {
            this.key = null;

            if (keyname == null)
            {
                return;
            }

            if (parent == null)
            {
                RegistryKey[] rootKeys = { Registry.CurrentUser };

                // see if the name begins with a root key name (eg "HKEY_CURRENT_USER")
                foreach (RegistryKey root in rootKeys)
                {
                    if (keyname.StartsWith(root.Name))
                    {
                        parent = root;
                        keyname = keyname.Substring(root.Name.Length).TrimStart(new char[] { '/', '\\' });
                        break;
                    }
                }

                if (parent == null)
                {
                    parent = rootKeys[0];
                }
            }

            try
            {
                RegistryKey key;
                if (write)
                {
                    key = parent.CreateSubKey(keyname);
                }
                else
                {
                    key = parent.OpenSubKey(keyname, RegistryKeyPermissionCheck.Default, System.Security.AccessControl.RegistryRights.ReadKey);
                }

                if (key != null)
                {
                    this.Location = new RegistryStoreLocation(key.Name);
                    this.canWrite = write;
                    this.key = key;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        /// <summary>Gets all value names.</summary>
        /// <returns>An array of value names.</returns>
        public override string[] GetAllValueNames()
        {
            if (!this.isValid)
            {
                return new string[0];
            }

            return this.key.GetValueNames();
        }

        /// <summary>Gets the value, as an object.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as an object.</returns>
        public override object GetObjectValue(string name, object defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            return this.key.GetValue(name, defaultValue) ?? defaultValue;
        }

        /// <summary>Gets a string value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a string.</returns>
        public override string GetValue(string name, string defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            object value = this.key.GetValue(name, defaultValue, RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (value == null)
            {
                return defaultValue;
            }

            return value as string ?? value.ToString();
        }

        /// <summary>Gets a numeric value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as an int.</returns>
        public override int GetValue(string name, int defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            object value = this.key.GetValue(name, defaultValue);

            if (value is int)
            {
                return (int)value;
            }

            string stringValue = value as string ?? value.ToString();

            if (stringValue.Length == 0)
            {
                return defaultValue;
            }

            int n;
            if (int.TryParse(stringValue, out n))
            {
                return n;
            }

            return defaultValue;
        }

        /// <summary>Gets a numeric value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a long int.</returns>
        public override long GetValue(string name, long defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            object value = this.key.GetValue(name, defaultValue);

            if ((value is long) || (value is int))
            {
                return (long)value;
            }

            string stringValue = value as string ?? value.ToString();

            if (stringValue.Length == 0)
            {
                return defaultValue;
            }

            long n;
            if (long.TryParse(stringValue, out n))
            {
                return n;
            }

            return defaultValue;
        }

        /// <summary>Gets a boolean value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value, as a boolean.</returns>
        public override bool GetValue(string name, bool defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            object value = this.key.GetValue(name, defaultValue);

            if (value is bool)
            {
                return (bool)value;
            }
            else if (value is int)
            {
                return (int)value != 0;
            }
            else if (value == null)
            {
                return defaultValue;
            }

            string stringValue = value as string ?? value.ToString();

            if (stringValue.Length == 0)
            {
                return defaultValue;
            }

            bool b;
            if (bool.TryParse(stringValue, out b))
            {
                return b;
            }

            string[] falseStrings = { bool.FalseString, "0", "false", "no", "n" };
            string[] trueStrings = { bool.TrueString, "1", "true", "yes", "y" };

            foreach (string s in falseStrings)
            {
                if (string.Compare(s, true.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }

            foreach (string s in trueStrings)
            {
                if (string.Compare(s, true.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return defaultValue;
        }

        /// <summary>Gets a byte array value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The value, as an array of bytes.
        /// </returns>
        public override byte[] GetValue(string name, byte[] defaultValue)
        {
            if (!this.isValid)
            {
                return defaultValue;
            }

            object value = this.key.GetValue(name, defaultValue);
            return value as byte[] ?? defaultValue;
        }

        /// <summary>Gets the value, of enumeration by TEnum.</summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public override TEnum GetValue<TEnum>(string name, TEnum defaultValue)
        {
            string stringValue = this.GetValue(name, (string)null);
            TEnum value = defaultValue;

            if (string.IsNullOrEmpty(stringValue))
            {
                return value;
            }

            if (Enum.TryParse<TEnum>(stringValue, out value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(string name, string value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value ?? string.Empty, RegistryValueKind.String);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(string name, int value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value, RegistryValueKind.DWord);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(string name, long value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value, RegistryValueKind.QWord);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(string name, bool value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(string name, byte[] value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value, RegistryValueKind.Binary);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public override void SetValue<TEnum>(string name, TEnum value)
        {
            if (this.canWrite && this.isValid)
            {
                this.key.SetValue(name, value.ToString());
            }
        }

        /// <summary>Creates a sub-store.</summary>
        /// <param name="name">The name.</param>
        /// <returns>A storage.</returns>
        public override Storage CreateSubStore(string name)
        {
            if (this.key == null)
            {
                return new RegistryStore();
            }

            return new RegistryStore(this.key, name, this.canWrite);
        }
    }
}