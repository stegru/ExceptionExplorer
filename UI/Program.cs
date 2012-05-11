namespace ExceptionExplorer
{
    using System;
    using System.Windows.Forms;
    using System.IO;
    using System.Reflection;
    using Decompiler;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using ExceptionExplorer.UI;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.Reflection.Emit;
    using System.Text;
    using ExceptionExplorer.Config;
    using Microsoft.WindowsAPICodePack.ApplicationServices;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Diagnostics;

    /// <summary>
    /// Program entry point
    /// </summary>
    internal static class Program
    {
        public static CommandLine CommandLine { get; private set; }

        public static ExceptionExplorerForm ExceptionExplorerForm { get; private set; }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                ErrorHandler.Init();

                CommandLine = new CommandLine();
                CommandLine.Parse(args);

                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "ILSpy");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Licence lic = Options.Current.Licence;
                Activation.CheckLicence(lic);

                if (lic.Status == Licence.LicenceStatus.Invalid)
                {
                }

                //Application.Run(new OpenSolutionForm()); return;

                ExceptionExplorerForm = new ExceptionExplorerForm();
                Application.Run(ExceptionExplorerForm);
                return;

                //ExceptionExplorer.ExceptionAnalysis.ExceptionFinder.Instance.ReadClass(new Class(typeof(ExceptionExplorerForm)), new System.Threading.CancellationToken(), true);
            }
            finally
            {
                ErrorHandler.DeInit();
            }
        }

        public static void Restart()
        {
            Restart(null);
        }

        public static void Restart(string extraParams)
        {
            StringBuilder commandLine = new StringBuilder();
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 1; i < args.Length; i++)
            {
                commandLine.Append('"');
                commandLine.Append(args[i]);
                commandLine.Append('"').Append(' ');
            }

            if (extraParams != null)
            {
                commandLine.Append(extraParams);
            }

            ProcessStartInfo startInfo = Process.GetCurrentProcess().StartInfo;
            startInfo.FileName = Application.ExecutablePath;

            if (commandLine.Length > 0)
            {
                startInfo.Arguments = commandLine.ToString().Trim();
            }

            Application.Exit();
            Process.Start(startInfo);
        }

    }

    internal class CommandLine : BaseCommandLine
    {
    }

    /// <summary>
    /// Command line parser.
    /// </summary>
    internal abstract class BaseCommandLine
    {
        /// <summary>
        /// A command line argument
        /// </summary>
        public class CommandLineArgument
        {
            /// <summary>
            /// Gets a value indicating whether this argument is set.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this argument is set; otherwise, <c>false</c>.
            /// </value>
            public bool IsSet { get; private set; }

            /// <summary>Gets the name.</summary>
            public string Name { get; private set; }

            /// <summary>Gets the value.</summary>
            public string Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommandLineArgument"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public CommandLineArgument(string name)
            {
                this.Name = name;
                this.IsSet = false;
                this.Value = null;
            }

            /// <summary>Sets the value.</summary>
            /// <param name="value">The value.</param>
            public void SetValue(string value)
            {
                this.Value = value;
                this.IsSet = this.Value != null;
            }
        }

        /// <summary>Gets the command line, as a string.</summary>
        public string CommandString { get; private set; }

        /// <summary>Gets all arguments.</summary>
        public Dictionary<string, string> AllArguments { get; private set; }

        /// <summary>Gets the unknown arguments.</summary>
        public Dictionary<string, string> UnknownArguments { get; private set; }

        /// <summary>Gets the command files.</summary>
        public List<string> CommandFiles { get; private set; }

        /// <summary>
        /// Gets the command string, without the /options.
        /// </summary>
        public string CommandStringNoOptions { get; private set; }

        /// <summary>The properties.</summary>
        private Dictionary<string, PropertyInfo> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommandLine"/> class.
        /// </summary>
        public BaseCommandLine()
        {
            this.AllArguments = new Dictionary<string, string>();
            this.CommandFiles = new List<string>();
            this.properties = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(CommandLineArgument))
                {
                    this.properties[pi.Name.ToUpperInvariant()] = pi;
                    pi.SetValue(this, new CommandLineArgument(pi.Name), null);
                }
            }
        }

        /// <summary>Sets the properties.</summary>
        private void SetProperties()
        {
            foreach (KeyValuePair<string, string> pair in this.AllArguments)
            {
                PropertyInfo pi;
                if (this.properties.TryGetValue(pair.Key.ToUpperInvariant(), out pi))
                {
                    CommandLineArgument cla = pi.GetValue(this, null) as CommandLineArgument;
                    if (cla != null)
                    {
                        cla.SetValue(pair.Value);
                    }
                }
                else
                {
                    if (this.UnknownArguments == null)
                    {
                        this.UnknownArguments = new Dictionary<string, string>();
                    }

                    this.UnknownArguments.Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>Parses the command-line arguments.</summary>
        public void Parse()
        {
            this.Parse(Environment.GetCommandLineArgs());
        }

        /// <summary>Parses the command-line arguments.</summary>
        /// <param name="args">The command-line arguments.</param>
        public void Parse(string[] args)
        {
            this.Parse(string.Join(" ", args));
        }

        /// <summary>Parses the command-line arguments.</summary>
        /// <param name="args">The command-line arguments.</param>
        public void Parse(string args)
        {
            this.CommandString = args;

            Regex re = new Regex(@"/\w+(=(([^""])\S*)|""([^""]|"""")*"")?");
            this.CommandStringNoOptions = re.Replace(this.CommandString, string.Empty);

            re = new Regex(
            @"
                (?:^|\s)+
                    (
                        /(?<opt>\w*)
                        (?:=(
                            (?<value>(?:[^\s""])\S*)
                                |
                            ""(?<value>([^""]|"""")*)""
                        ))?
                    |
                        (?<file>(?:[^\s""/])\S*)
                        |
                        ""(?<file>([^""]|"""")*)""
                    )
                ", RegexOptions.IgnorePatternWhitespace);

            MatchCollection matches = re.Matches(this.CommandString);

            foreach (Match m in matches)
            {
                Group option = m.Groups["opt"];
                if (option.Success)
                {
                    Group value = m.Groups["value"];
                    this.AllArguments[option.Value] = value.Success ? value.Value : null;
                }
                else
                {
                    Group file = m.Groups["file"];
                    this.CommandFiles.Add(file.Value);
                }
            }

            this.SetProperties();
        }
    }


}