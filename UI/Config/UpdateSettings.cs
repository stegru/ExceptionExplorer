using System;

namespace ExceptionExplorer.Config
{
    public class UpdateSettings : OptionsBase
    {
        public Setting<string> Latest { get; private set; }

        public Setting<string> UpgradeURL { get; private set; }

        public Setting<DateTime> LastChecked { get; private set; }

        public Setting<VersionCheckResponse> LastResponse { get; private set; }

        public Setting<VersionCheckFrequency> CheckFrequency { get; private set; }

        public UpdateSettings()
        {
            this.CheckFrequency.Value = VersionCheckFrequency.Weekly;
        }
    }
}