namespace ExceptionExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Windows.Forms;
    using Microsoft.WindowsAPICodePack.ApplicationServices;
    using ExceptionExplorer.General;
    using System.Net;
    using ExceptionExplorer.Config;
    using ExceptionExplorer.UI;
    using System.Threading;
using System.Xml.Serialization;
    using System.Xml;
    using Microsoft.Win32;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Error handing.
    /// </summary>
    public class ErrorHandler
    {
        #region Static
        /// <summary>Initialises the error handling.</summary>
        public static void Init()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("/restart", RestartRestrictions.NotOnPatch | RestartRestrictions.NotOnReboot));
        }

        /// <summary>De-initialises the error handling.</summary>
        public static void DeInit()
        {
            Application.ThreadException -= Application_ThreadException;
            ApplicationRestartRecoveryManager.UnregisterApplicationRestart();
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException(e.ExceptionObject as Exception, false);
        }

        /// <summary>
        /// Handles the ThreadException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Threading.ThreadExceptionEventArgs"/> instance containing the event data.</param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            UnhandledException(e.Exception, true);
        }
        #endregion

        private static void UnhandledException(Exception exception, bool canResume)
        {
            if (exception == null)
            {
                return;
            }

            ErrorHandler handler = new ErrorHandler(exception);
            handler.Show(canResume);
        }

        TaskDialog errorDialog;
        TaskDialog reportDialog;
        Exception exception;
        string reportContent;
        WebClient webClient;
        bool restart = false;
        bool reportDefault = false;


        private ErrorHandler(Exception exception)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Options.RegistryRootKeyName, false))
            {
                object value = key.GetValue("ReportErrors", null);
                if (value == null)
                {
                    this.reportDefault = (new Random()).Next() % 2 == 0;
                }
                else if (value is int)
                {
                    this.reportDefault = (int)value != 0;
                }
            }

            this.exception = exception;
        }

        private void Show(bool canResume)
        {
            this.errorDialog = new TaskDialog()
            {
                Caption = "Exception Explorer",
                InstructionText = "An error has occurred.",
                Text = this.exception.Message + "\n\n" + this.exception.GetType().FullName,
                Icon = TaskDialogStandardIcon.Error,
                StandardButtons = TaskDialogStandardButtons.Close,
                FooterCheckBoxText = "Send an error report",
                FooterCheckBoxChecked = this.reportDefault,
                Cancelable = true,
            };

            this.errorDialog.Opened += (sender, e) => { this.errorDialog.Icon = this.errorDialog.Icon; };

            if (canResume)
            {
                TaskDialogCommandLink continueLink = new TaskDialogCommandLink("continue", "Continue");
                continueLink.Click += (sender, e) =>
                {
                    this.errorDialog.Close();
                };

                TaskDialogCommandLink restartLink = new TaskDialogCommandLink("restart", "Restart");
                restartLink.Click += (sender, e) =>
                {
                    this.restart = true;
                    this.errorDialog.Close();
                };

                this.errorDialog.Controls.Add(continueLink);
                this.errorDialog.Controls.Add(restartLink);
            }

            this.errorDialog.Show(Program.ExceptionExplorerForm);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(Options.RegistryRootKeyName))
            {
                key.SetValue("ReportErrors", this.errorDialog.FooterCheckBoxChecked.Value ? 1 : 0);
            }

            if (this.errorDialog.FooterCheckBoxChecked.Value == true)
            {
                this.SendErrorReport();
            }

            if (this.restart)
            {
                Program.Restart("/restart");
            }
        }

        private void SendErrorReport()
        {
            this.reportDialog = new TaskDialog()
            {
                Caption = "Exception Explorer",
                InstructionText = "Submitting error report...",
                ProgressBar = new TaskDialogProgressBar("progress")
                {
                    State = TaskDialogProgressBarState.Marquee
                },
                Icon = TaskDialogStandardIcon.None,
                StandardButtons = TaskDialogStandardButtons.Close,
                Cancelable = true,
            };

            this.reportDialog.Opened += (sender, e) =>
            {
                this.reportContent = this.CreateReport();
                this.reportDialog.Icon = this.reportDialog.Icon;
                this.webClient = new WebClient();
                new Thread(this.UploadReport).Start();
            };

            this.reportDialog.Show(this.errorDialog.OwnerWindowHandle);
        }

        
        public class ErrorReport
        {
            private string exceptionDetail;

            public ErrorReport() { }

            public ErrorReport(Exception exception)
            {
                this.exceptionDetail = exception.ToString();
                this.Exception = exception.GetType().FullName;
                this.Message = exception.Message;
                this.Hash = this.exceptionDetail.Hash();
                this.Version = AppVersion.Current.ToString();
                this.OSVersion = Environment.OSVersion.VersionString;
                this.CLRVersion = Environment.Version.ToString();
                this.CommandLine = Environment.CommandLine;

                this.Assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(ass => ass.FullName).ToArray();

            }

            public string ExceptionDetail
            {
                get
                {
                    return ToBase64(this.exceptionDetail);
                }
                set
                {
                    this.exceptionDetail = FromBase64(value);
                }
            }

            public string Version { get; set; }
            public string Exception { get; set; }
            public string Message { get; set; }
            public string Hash { get; set; }
            public string OSVersion { get; set; }
            public string CLRVersion { get; set; }
            public string CommandLine { get; set; }

            public string[] Assemblies { get; set; }
        }

        private static string ToBase64(string text)
        {
            return Encoding.UTF8.GetBytes(text).ToBase64();
        }
        private static string FromBase64(string text)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        private string CreateReport()
        {
            const bool format = true;

            ErrorReport report = new ErrorReport(this.exception);
            

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer serialiser = new XmlSerializer(report.GetType());

            StringBuilder xmlText = new StringBuilder();

            XmlWriterSettings settings = format
                ? new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n",
                    OmitXmlDeclaration = true,
                    NewLineHandling = NewLineHandling.Replace
                }
                : new XmlWriterSettings()
                {
                    Indent = false,
                    IndentChars = "",
                    NewLineChars = "",
                    OmitXmlDeclaration = true,
                    NewLineHandling = NewLineHandling.None
                };

            settings.Indent = format;
            using (XmlWriter writer = XmlWriter.Create(xmlText, settings))
            {
                serialiser.Serialize(writer, report, ns);
            }

            return xmlText.ToString();
        }

        private void UploadReport()
        {
            Exception error = null;
            try
            {
                string address = Web.GetSiteUrl(SiteLink.ErrorReport);
                this.webClient.Headers.Add(string.Format("User-Agent: ExceptionExplorer/{0} (error reporter)", AppVersion.Current));
                this.webClient.Proxy = WebRequest.GetSystemWebProxy();
                byte[] data = Compress(this.reportContent);
                this.webClient.UploadData(address, data);
            }
            catch (WebException we)
            {
                error = we;
            }
            finally
            {
                try
                {
                    this.reportDialog.Close();

                    if (error == null)
                    {
                        Dialog.Show(null, "The error has been reported.", "Thank you", TaskDialogStandardButtons.Close, TaskDialogStandardIcon.Information);
                    }
                    else
                    {
                        Dialog.Show(null, "There was a problem submitting the report", error.Message, TaskDialogStandardButtons.Close, TaskDialogStandardIcon.Warning);
                    }
                }
                catch (InvalidOperationException)
                {
                    // dialog was already closed
                }
            }
        }

        private static byte[] Compress(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (MemoryStream input = new MemoryStream(bytes))
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(output, CompressionMode.Compress))
                {
                    input.CopyTo(gz);
                }

                return output.ToArray();
            }
        }
    }
}
