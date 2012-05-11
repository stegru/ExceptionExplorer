// -----------------------------------------------------------------------
// <copyright file="AssemblyLoader.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.ExceptionAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.IO;
    using System.Configuration;
    using System.Threading;
    using System.Windows.Forms;
    using System.Collections;
using ExceptionExplorer.UI;
using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// Loads assemblies for analysis.
    /// </summary>
    internal class AssemblyLoader
    {
        private static AssemblyLoader instance;

        private ThreadLocal<Assembly> currentMainAssembly = new ThreadLocal<Assembly>();

        private List<string> recentPaths = new List<string>();

        public Assembly CurrentAssembly
        {
            get
            {
                return this.currentMainAssembly.IsValueCreated ? this.currentMainAssembly.Value : null;
            }
            set
            {
                this.currentMainAssembly.Value = value;
            }
        }

        /// <summary>Initialises the assembly loader.</summary>
        internal static void Init()
        {
            if (instance == null)
            {
                instance = new AssemblyLoader();
            }
        }

        /// <summary>Prevents a default instance of the <see cref="AssemblyLoader"/> class from being created.</summary>
        private AssemblyLoader()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
            try
            {
                Decompiler.SourceViewer.SetAssemblyResolveCallback(this.ResolveAssembly);
            }
            catch (TypeLoadException)
            {
                //
            }
        }

        private Assembly LoadFromGAC(AssemblyName name)
        {
            byte[] pk = name.GetPublicKeyToken();
            if ((pk == null) || (pk.Length == 0))
            {
                // public keys are needed, if the assembly is in the GAC
                return null;
            }

            try
            {
                return Assembly.ReflectionOnlyLoad(name.FullName);
            }
            catch (IOException) { }
            catch (BadImageFormatException) { }
            catch (ArgumentException) { }

            return null;
        }

        /// <summary>
        /// Handles the ReflectionOnlyAssemblyResolve event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.ResolveEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return this.ResolveAssembly(new AssemblyName(args.Name), args.RequestingAssembly);
        }

        /// <summary>Resolves an assembly.</summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="requestingAssembly">The requesting assembly.</param>
        /// <returns></returns>
        private Assembly ResolveAssembly(AssemblyName name, Assembly requestingAssembly)
        {
            Assembly asm = this._ResolveAssembly(name, requestingAssembly);
            if (asm != null)
            {
                string path = Path.GetDirectoryName(asm.Location);

                this.recentPaths.Remove(path);
                this.recentPaths.Add(path);
            }

            return asm;
        }

        /// <summary>Resolves an assembly.</summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="requestingAssembly">The requesting assembly.</param>
        /// <returns></returns>
        private Assembly _ResolveAssembly(AssemblyName name, Assembly requestingAssembly)
        {
            /*
             * .NET search path:
             * ./name.dll
             * ./name/name.dll
             * ./name.exe
             * ./name/name.exe
             */
            Assembly asm = null;

            asm = this.LoadFromGAC(name);

            if (asm != null)
            {
                return asm;
            }

            string[] files = new string[] {
                string.Format("{0}.", name.Name),
                string.Format("{0}{1}{0}.", name.Name, Path.PathSeparator),
            };

            IEnumerable<string> paths = new List<string>();
            if (requestingAssembly != null)
            {
                ((IList)paths).Add(Path.GetDirectoryName(requestingAssembly.Location));
                //paths.AddRange(this.GetAssemblyLoadPaths(args.RequestingAssembly));
            }

            paths = paths.Concat(recentPaths);


            foreach (string ext in new string[] { "dll", "exe" })
            {
                foreach (string path in paths)
                {
                    foreach (string file in files)
                    {
                        try
                        {
                            string filename = Path.Combine(path, file + ext);
                            if (File.Exists(filename))
                            {
                                // todo: check if it's the correct assembly
                                asm = Assembly.ReflectionOnlyLoadFrom(filename);
                                if (asm != null)
                                {
                                    return asm;
                                }
                            }
                        }
                        catch (IOException) { }
                        catch (BadImageFormatException) { }
                        catch (ArgumentException) { }
                    }
                }
            }

            // ask the user.
            if (Program.ExceptionExplorerForm != null)
            {
                asm = null;
                while (asm == null)
                {
                    string path = Program.ExceptionExplorerForm.GetAssemblyPath(requestingAssembly, name);
                    string errorMessage = null;
                    if (path == null)
                    {
                        return null;
                    }

                    try
                    {
                        asm = Assembly.ReflectionOnlyLoadFrom(path);
                        if (asm.FullName != name.FullName)
                        {
                            Func<TaskDialogResult> msg = () =>
                            {
                                return Dialog.Show(Program.ExceptionExplorerForm,
                                    "Incorrect File", 
                                    string.Format("The file '{0}' does not look like the one being requested.\n\nDo you still want to use this file?", Path.GetFileName(path)),
                                    string.Format(
@"Requested file:
{0}.dll: {1}

Selected file:
{2}: {3}
", name.Name, name.FullName, Path.GetFileName(path), asm.FullName),
                                    TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No | TaskDialogStandardButtons.Cancel, TaskDialogStandardIcon.Warning);
                            };

                            DialogResult result = (DialogResult)Program.ExceptionExplorerForm.Invoke(msg);
                            switch (result)
                            {
                                case DialogResult.Yes:
                                    return asm;
                                case DialogResult.No:
                                    asm = null;
                                    break;
                                case DialogResult.Cancel:
                                    return null;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        errorMessage = string.Format("{0}\n\nCould not read the file: \n{1}", path, ex.Message);
                    }
                    catch (BadImageFormatException)
                    {
                        errorMessage = string.Format("{0}\n\bThis assembly is not in a known format.", path);
                    }
                    catch (ArgumentException)
                    {
                        errorMessage = string.Format("Invalid filename provided:\n'{0}'\n\n{1}", path);
                    }
                }
            }

            return asm;
        }

        private string[] GetAssemblyLoadPaths(Assembly assembly)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);

            if ((config == null) || !config.HasFile)
            {
                return null;
            }

            return null;
        }



    }


}
