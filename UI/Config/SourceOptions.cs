namespace ExceptionExplorer.Config
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SourceOptions : OptionsBase
    {
        public Setting<bool> ShowXmlDoc { get; private set; }

        public Setting<bool> ShowUsing { get; private set; }

        public Setting<bool> DecompileLanguageFeatures { get; private set; }

        public Setting<bool> AutoDecompile { get; private set; }

        public SourceOptions()
        {
            this.DecompileLanguageFeatures.Value =
                this.ShowUsing.Value =
                this.ShowXmlDoc.Value =
                this.AutoDecompile.Value = true;
        }
    }
}