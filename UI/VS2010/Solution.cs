namespace ExceptionExplorer.VS2010
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 
    /// </summary>
    public class Solution
    {
        public string SlnFile { get; private set; }
        public string SolutionBaseDirectory { get; private set; }
        public List<string> ProjectFiles { get; private set; }
        public string Name { get; set; }
        public bool Success { get; private set; }

        public Solution(string file)
        {
            this.Success = false;
            this.SlnFile = file;
            this.SolutionBaseDirectory = Path.GetDirectoryName(file);
            this.ProjectFiles = new List<string>();
            this.Name = Path.GetFileNameWithoutExtension(file);
            this.ReadFile();
        }

        private void ReadFile()
        {
            
            Regex projectRegex = new Regex(@"^Project\([^\)]*\)\s*=\s*""(?<name>[^""]*)""\s*,\s*""(?<file>[^""]*)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            try
            {
                using (StreamReader reader = new StreamReader(this.SlnFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();

                        if (line.StartsWith("Project", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Match m = projectRegex.Match(line);
                            if (m.Success)
                            {
                                string file = m.Groups["file"].Value;
                                if (!string.IsNullOrEmpty(file))
                                {
                                    if (!Path.IsPathRooted(file))
                                    {
                                        file = Path.Combine(this.SolutionBaseDirectory, file);
                                    }

                                    if (File.Exists(file))
                                    {
                                        this.ProjectFiles.Add(file);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                return;
            }

            this.Success = true;
        }

        public IEnumerable<Project> GetProjects()
        {
            foreach (string file in this.ProjectFiles)
            {
                Project p = new Project(file);
                if (p.IsValid)
                {
                    yield return p;
                }
            }
        }
    }
}
