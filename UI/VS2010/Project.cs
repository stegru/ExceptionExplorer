namespace ExceptionExplorer.VS2010
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Xsl;
    using System.Text.RegularExpressions;
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    public enum ProjectOutputType
    {
        Unknown = 0,
        Library = 1,
        Exe = 2,
        Module = 3,
        Winexe = 4
    }

    public class Project
    {
        public class Output
        {
            public string Configuration { get; set; }
            public string Platform { get; set; }
            public string OutputPath { get; set; }
            public string OutputFile { get; set; }
            public ProjectOutputType OutputType { get; set; }
        }

        public string ProjFile { get; private set; }
        public string AssemblyName { get; private set; }
        public string Name { get; private set; }
        public ProjectOutputType OutputType { get; private set; }

        public bool IsValid { get; private set; }

        private Dictionary<string, List<string>> configPlatforms;
        private Dictionary<string, Output> outputs;
        public string DefaultConfig;
        public string DefaultPlatform;

        public string GetOutputFile(string config, string platform)
        {
            string key = string.Format("{0}|{1}", config, platform);
            
            Output output;
            if (!this.outputs.TryGetValue(key, out output))
            {
                return null;
            }

            return output.OutputFile;
        }

        public string[] GetConfigurations()
        {
            return this.configPlatforms.Keys.ToArray();
        }

        public string[] GetPlatforms(string config)
        {
            return this.configPlatforms[config].ToArray();
        }

        public Project(string projFile)
        {
            this.ProjFile = projFile;
            this.ReadFile();
        }

        private static string GetOutputTypeExtension(ProjectOutputType outputType)
        {
            switch (outputType)
            {
                case ProjectOutputType.Exe:
                case ProjectOutputType.Winexe:
                    return "exe";
                case ProjectOutputType.Library:
                case ProjectOutputType.Module:
                    return "dll";
            }

            return "";
        }

        private void ReadFile()
        {
            Regex configPlatormRegex = new Regex(@"'\$\(Configuration\)\|\$\(Platform\)' == '(?<config>[^|']*)\|(?<platform>[^']*)'");

            try
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(this.ProjFile);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("b", doc.DocumentElement.NamespaceURI);

                XmlElement project = doc.DocumentElement;
                XmlNode node;

                this.DefaultPlatform = this.DefaultConfig = "";
                this.configPlatforms = new Dictionary<string, List<string>>();
                this.Name = Path.GetFileNameWithoutExtension(this.ProjFile);
                this.outputs = new Dictionary<string, Output>();

                node = project.SelectSingleNode(@"b:PropertyGroup/b:AssemblyName", nsmgr);
                if (node != null)
                {
                    this.AssemblyName = node.InnerText;
                }

                node = project.SelectSingleNode("b:PropertyGroup/b:OutputType", nsmgr);
                if (node != null)
                {
                    ProjectOutputType type = ProjectOutputType.Unknown;
                    Enum.TryParse<ProjectOutputType>(node.InnerText, true, out type);
                    this.OutputType = type;
                }

                // Get the configurations and platforms
                foreach (XmlNode condition in project.SelectNodes(@"b:PropertyGroup/@Condition", nsmgr))
                {
                    Match m = configPlatormRegex.Match(condition.Value);
                    if (m.Success)
                    {
                        if (m.Groups["config"].Success && m.Groups["platform"].Success)
                        {
                            string config = m.Groups["config"].Value;
                            string platform = m.Groups["platform"].Value;

                            List<string> platforms;
                            if (!this.configPlatforms.TryGetValue(config, out platforms))
                            {
                                platforms = new List<string>();
                                this.configPlatforms.Add(config, platforms);
                            }

                            platforms.Add(platform);

                            string outputPath = "";
                            node = condition.SelectSingleNode("../b:OutputPath", nsmgr);
                            if (node != null)
                            {
                                outputPath = node.InnerText;
                                if (!Path.IsPathRooted(outputPath))
                                {
                                    outputPath = Path.Combine(Path.GetDirectoryName(this.ProjFile), outputPath);
                                }
                            }

                            node = condition.SelectSingleNode("../b:OutputType", nsmgr);
                            ProjectOutputType outputType = this.OutputType;
                            if (node != null)
                            {
                                Enum.TryParse<ProjectOutputType>(node.InnerText, out outputType);
                            }

                            Output output = new Output()
                            {
                                Configuration = config,
                                Platform = platform,
                                OutputPath = outputPath,
                                OutputType = outputType,
                                OutputFile = Path.Combine(outputPath, this.AssemblyName + "." + GetOutputTypeExtension(outputType))
                            };

                            this.outputs.Add(string.Format("{0}|{1}", config, platform), output);
                        }
                    }
                }

                if (this.configPlatforms.Count == 0)
                {
                    // no configurations found
                    return;
                }

                this.DefaultConfig = this.configPlatforms.Keys.First();
                this.DefaultPlatform = this.configPlatforms.First().Value.First();

                // get the default config/platform
                node = project.SelectSingleNode(@"b:PropertyGroup/b:Configuration", nsmgr);
                if (node != null)
                {
                    this.DefaultConfig = node.InnerText;
                }

                node = project.SelectSingleNode(@"b:PropertyGroup/b:Platform", nsmgr);
                if (node != null)
                {
                    this.DefaultPlatform = node.InnerText;
                }

                this.IsValid = true;

            }
            catch (XmlException)
            {
            }


            return;

        }
    }
}
