// -----------------------------------------------------------------------
// <copyright file="LatestVersion.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Config
{
    using System;
    using System.Reflection;
    using ExceptionExplorer.UI.Jobs;
    using System.Net;
    using System.Windows.Forms;
    using System.IO;
    using System.Text.RegularExpressions;
    using ExceptionExplorer.UI;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using ExceptionExplorer.General;

    public enum VersionCheckResponse
    {
        OK = 0,
        NewVersion,
        Failed
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class AppVersion
    {
        private static Version current;

        public static bool UpgradeRequired
        {
            get
            {
                return (Latest > Current);
            }
        }

        public static bool CheckRequired
        {
            get
            {
                VersionCheckFrequency freq = Options.Current.Update.CheckFrequency.Value;
                if (freq == VersionCheckFrequency.Never)
                {
                    return false;
                }

                DateTime lastCheck = Options.Current.Update.LastChecked.Value;
                if (lastCheck == DateTime.MinValue)
                {
                    return true;
                }

                TimeSpan span = DateTime.Now - Options.Current.Update.LastChecked;
                double days = 10;

                switch (freq)
                {
                    case VersionCheckFrequency.Daily:
                        days = 1;
                        break;

                    case VersionCheckFrequency.Weekly:
                        days = 7;
                        break;

                    case VersionCheckFrequency.Monthly:
                        days = 30;
                        break;
                }

                if (Options.Current.Update.LastResponse.Value == VersionCheckResponse.Failed)
                {
                    days = days * 0.66;
                }

                return span.TotalDays >= days;
            }
        }

        /// <summary>Gets the current version (this assembly).</summary>
        public static Version Current
        {
            get
            {
                return current ?? (current = Assembly.GetExecutingAssembly().GetName().Version);
            }
        }

        /// <summary>Gets the latest version.</summary>
        public static Version Latest
        {
            get
            {
                Version v;
                if (Version.TryParse(Options.Current.Update.Latest.Value, out v))
                {
                    return v;
                }
                else
                {
                    return AppVersion.Current;
                }
            }
        }

        public class VersionCheckState
        {
            public Action<VersionCheckState> Callback { get; set; }

            public bool Quiet { get; set; }

            public VersionCheckResponse Response { get; set; }

            public IWin32Window OwnerWindow { get; set; }

            public string Data { get; set; }

            public WebException Exception { get; set; }

            public string ChangesUrl { get; set; }
        }

        private static object getLatestLock = new object();

        private static IWin32Window _owner = null;

        public static void GetLatestWithUI(IWin32Window owner)
        {
            lock (getLatestLock)
            {
                _owner = owner;
                TaskDialog td = new TaskDialog()
                {
                    OwnerWindowHandle = owner == null ? IntPtr.Zero : owner.Handle,
                    Caption = "Exception Explorer",
                    InstructionText = "Checking for updates...",
                    StandardButtons = TaskDialogStandardButtons.Cancel,
                    Cancelable = true,
                    ProgressBar = new TaskDialogProgressBar("progress")
                    {
                        State = TaskDialogProgressBarState.Marquee
                    }
                };

                td.Opened += new EventHandler(GetLatestDialog_Opened);

                td.Show();
            }
        }

        private static void GetLatestDialog_Opened(object sender, EventArgs e)
        {
            TaskDialog td = (TaskDialog)sender;

            GetLatestAsync(_owner, (VersionCheckState state) =>
            {
                ShowResponseDialog(state);
                td.Close();
            }, true);
        }

        private static void ShowResponseDialog(VersionCheckState state)
        {
            ShowResponseDialog(state.OwnerWindow, state.Exception == null ? null : state.Exception.Message);
        }

        public static void ShowResponseDialog(IWin32Window owner)
        {
            ShowResponseDialog(owner, null);
        }

        private static void ShowResponseDialog(IWin32Window owner, string error)
        {
            TaskDialog td = new TaskDialog()
            {
                OwnerWindowHandle = owner == null ? IntPtr.Zero : owner.Handle,
                Caption = "Exception Explorer",
                Cancelable = true
            };

            td.Opened += (sender, args) => td.Icon = td.Icon;

            VersionCheckResponse response = Options.Current.Update.LastResponse.Value;
            if (AppVersion.UpgradeRequired)
            {
                response = VersionCheckResponse.NewVersion;
            }

            switch (response)
            {
                case VersionCheckResponse.Failed:
                    td.Icon = TaskDialogStandardIcon.Warning;
                    td.InstructionText = "Could not find the latest version.";
                    td.DetailsExpandedText = error;
                    break;

                case VersionCheckResponse.OK:
                    td.Icon = TaskDialogStandardIcon.Information;
                    td.InstructionText = "You are already running the latest version.";
                    break;

                case VersionCheckResponse.NewVersion:
                    td.Icon = TaskDialogStandardIcon.Information;
                    td.InstructionText = "A new version is available.";
                    td.DetailsExpandedText = string.Format("Version {0} is available.\nYou are running {1}.", AppVersion.Latest, AppVersion.Current);

                    TaskDialogCommandLink download = new TaskDialogCommandLink("name", "Download now");
                    download.Click += (sender, args) =>
                    {
                        Web.OpenSite(SiteLink.NewVersion);
                        td.Close();
                    };

                    TaskDialogCommandLink later = new TaskDialogCommandLink("name", "Remind me later");
                    later.Click += (sender, args) =>
                    {
                        td.Close();
                    };

                    //td.FooterCheckBoxText = "Don't remind me of this version";

                    td.Controls.Add(download);
                    td.Controls.Add(later);

                    break;
            }

            td.Show();
        }

        public static void GetLatestAsync(IWin32Window owner, Action<VersionCheckState> callback, bool quiet)
        {
            Options.Current.Update.LastChecked.Value = DateTime.Now;

            Job.Download.NewJob(null, (cancelToken) =>
            {
                string currentVersion = string.Format("{0}.{1}.{2}.{3}", Current.Major, Current.Minor, Current.Build, Current.Revision);
                string address = Web.GetSiteUrl(SiteLink.CheckVersion);

                VersionCheckState state = new VersionCheckState()
                {
                    Callback = callback,
                    Quiet = quiet,
                    Data = "",
                    Response = VersionCheckResponse.Failed,
                    OwnerWindow = owner
                };

                try
                {
                    Uri url = new Uri(address);
                    WebClient web = new WebClient();

                    web.Headers.Add(string.Format("User-Agent: ExceptionExplorer/{0} (version checker)", currentVersion));

                    state.Data = web.DownloadString(url);
                    state.Response = VersionCheckResponse.OK;
                }
                catch (WebException ex)
                {
                    state.Exception = ex;
                }

                DownloadStringCompleted(state);
                Options.Current.Update.LastResponse.Value = state.Response;

                Control control = owner as Control;
                if (!quiet && (control != null))
                {
                    control.InvokeIfRequired(() => ShowResponseDialog(state));
                }
            }).Execute();
        }

        private static void DownloadStringCompleted(VersionCheckState state)
        {
            if (state.Response != VersionCheckResponse.Failed)
            {
                state.Response = VersionCheckResponse.OK;

                string[] lines = state.Data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] a = line.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (a.Length == 0)
                    {
                        continue;
                    }

                    if (!GotResponseData(a[0], (a.Length > 1) ? a[1] : "", state))
                    {
                        break;
                    }
                }
            }

            if (state.Callback != null)
            {
                Control control = state.OwnerWindow as Control;
                if ((control != null) && control.InvokeRequired)
                {
                    control.Invoke(state.Callback, new object[] { state });
                }
                else
                {
                    state.Callback(state);
                }
            }
        }

        private static bool GotResponseData(string name, string value, VersionCheckState state)
        {
            switch (name.ToLowerInvariant())
            {
                case "version":
                    try
                    {
                        Options.Current.Update.Latest.Value = value;
                        if (AppVersion.UpgradeRequired)
                        {
                            state.Response = VersionCheckResponse.NewVersion;
                        }
                    }
                    catch (FormatException)
                    {
                    }
                    break;

                case "url":
                    Options.Current.Update.UpgradeURL.Value = value;
                    break;

                case "changelog":
                    state.ChangesUrl = value;
                    break;

                case "message":
                    if (!state.Quiet)
                    {
                        Dialog.Show(state.OwnerWindow, value, TaskDialogStandardButtons.Ok, TaskDialogStandardIcon.Information);
                    }

                    break;

                case "ask":
                    if (!state.Quiet)
                    {
                        if (Dialog.Show(state.OwnerWindow, value, TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No, TaskDialogStandardIcon.Information) == TaskDialogResult.No)
                        {
                            return false;
                        }
                    }

                    break;

                case "end":
                    return false;

                case "open":
                    if (!state.Quiet && Regex.IsMatch(value, "^[a-z0-9/]$"))
                    {
                        string address = Path.Combine(Web.GetSiteUrl(SiteLink.None), value).Replace(Path.DirectorySeparatorChar, '/');
                        Web.OpenUrl(address);
                    }

                    break;
            }
            return true;
        }
    }
}