// -----------------------------------------------------------------------
// <copyright file="Options.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Config
{
    public enum VersionCheckFrequency
    {
        Never = 0,
        Daily,
        Weekly,
        Monthly
    }

    public class Options : OptionsBase
    {
        private const string RegistryRootKey = @"software\ExceptionExplorer";
        private readonly IStoreLocation location = new RegistryStore.RegistryStoreLocation(RegistryRootKey);

        public static string RegistryRootKeyName
        {
            get
            {
                return RegistryRootKey;
            }
        }

        /// <summary>Gets or sets the storage location.</summary>
        /// <value>The location.</value>
        protected override IStoreLocation Location
        {
            get
            {
                return this.location;
            }
            set
            {
            }
        }

        private static Options _current;

        public static Options Current
        {
            get
            {
                return _current ?? (_current = new Options());
            }
        }

        /// <summary>Gets the analysis related options.</summary>
        public AnalysisOptions AnalysisOptions { get; private set; }

        /// <summary>Gets the window positions.</summary>
        //public WindowPositions WindowPositions { get; private set; }

        /// <summary>Gets the licence.</summary>
        public Licence Licence { get; set; }

        public Persistence Persistence { get; private set; }

        public Setting<byte[]> InstallInfo { get; set; }

        /// <summary>Gets the visibility of the show source panel.</summary>
        /// <value>The show source setting.</value>
        public Setting<bool> ShowSource { get; private set; }

        /// <summary>Gets the value whether to show the waiting admination.</summary>
        public Setting<bool> ShowWaiting { get; private set; }

        /// <summary>Gets a value indicating whether to show inherited members in the class tree.</summary>
        /// <value><c>true</c> to show inherited members; otherwise, <c>false</c>.</value>
        public Setting<bool> SeparateBaseClassItem { get; private set; }

        /// <summary>Gets a value indicating whether to show the full name of inherited members in the class tree.</summary>
        /// <value><c>true</c> to show the full name of inherited members; otherwise, <c>false</c>.</value>
        public Setting<bool> InheritedMemberFullname { get; private set; }

        /// <summary>Gets a value indicating whether to show the full name of inherited members in the class tree.</summary>
        /// <value><c>true</c> to show the full name of inherited members; otherwise, <c>false</c>.</value>
        public Setting<bool> MemberFullname { get; private set; }

        public Setting<bool> ShowStartWindow { get; private set; }

        public UpdateSettings Update { get; private set; }

        public SourceOptions Source { get; private set; }

        /// <summary>Gets the most recently used file list.</summary>
        public Setting<MRU> MRU { get; private set; }

        /// <summary>Saves this instance.</summary>
        public void Save()
        {
            using (Storage s = this.GetStore(true))
            {
                this.Save(s);
            }
        }

        /// <summary>Loads this instance.</summary>
        public void Load()
        {
            using (Storage s = this.GetStore(false))
            {
                this.Load(s);
            }
        }

        public Options()
            : base()
        {
            this.Licence = new Licence();

            this.ShowSource.Value = true;
            this.InstallInfo.Value = new byte[0];
            this.ShowWaiting.Value = true;
            this.ShowStartWindow.Value = true;

            this.Persistence = new Persistence();
            this.Source = new SourceOptions();
            this.AnalysisOptions = new AnalysisOptions()
            {
            };

            this.MRU.Value = new MRU("MRU");

            this.Update = new UpdateSettings();

            this.Load();
        }
    }
}