// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    // NEW STYLE
#if true
#else
    // Not sealed since the C++ compiler inherits from it
    public abstract class CCompiler : C.Compiler, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        protected Opus.Core.StringArray CommonIncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }

        private class ComposerXEDetails
        {
            public ComposerXEDetails(string version,
                              string gxxIncludePath,
                              string target)
            {
                if (null == version)
                {
                    throw new Opus.Core.Exception("Unable to determine ComposerXE version", false);
                }
                if (null == target)
                {
                    throw new Opus.Core.Exception("Unable to determine ComposerXE target", false);
                }

                this.Version = version;
                this.GxxIncludePath = gxxIncludePath;
                this.Target = target;
            }

            public string Version
            {
                get;
                private set;
            }

            public string GxxIncludePath
            {
                get;
                private set;
            }

            public string Target
            {
                get;
                private set;
            }
        }

        private static System.Collections.Generic.Dictionary<Opus.Core.Target, ComposerXEDetails> ComposerXEDetailsForTarget = new System.Collections.Generic.Dictionary<Opus.Core.Target, ComposerXEDetails>();

        private void GetComposerXEDetails(Opus.Core.Target target)
        {
            string pathPrefix = null;
            string gxxIncludeDir = null;
            string ComposerXETarget = null;
            string ComposerXEVersion = null;

            // get version
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.Executable(target);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Arguments = "-dumpversion";

                System.Diagnostics.Process process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new Opus.Core.Exception(System.String.Format("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName), false);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to execute '{0}'", processStartInfo.FileName), false);
                }

                ComposerXEVersion = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                ComposerXEVersion = ComposerXEVersion.Trim();
            }

            // get paths and targets
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = this.Executable(target);
                processStartInfo.ErrorDialog = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.Arguments = "-v";

                System.Diagnostics.Process process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processStartInfo);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    throw new Opus.Core.Exception(System.String.Format("'{0}': process filename '{1}'", ex.Message, processStartInfo.FileName), false);
                }

                if (null == process)
                {
                    throw new Opus.Core.Exception(System.String.Format("Unable to execute '{0}'", processStartInfo.FileName), false);
                }

                string details = process.StandardError.ReadToEnd();
                process.WaitForExit();

                string[] splitDetails = details.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

                foreach (string detail in splitDetails)
                {
                    string configuredWith = "Configured with: ";
                    if (detail.StartsWith(configuredWith))
                    {
                        string configuredOptions = detail.Substring(configuredWith.Length);
                        string[] splitConfigureOptions = configuredOptions.Split(' ');

                        const string pathPrefixKey = "--prefix=";
                        const string gxxIncludeDirKey = "--with-gxx-include-dir=";
                        const string targetKey = "--target=";
                        foreach (string option in splitConfigureOptions)
                        {
                            if (option.StartsWith(pathPrefixKey))
                            {
                                pathPrefix = option.Substring(pathPrefixKey.Length).Trim();;
                            }
                            else if (option.StartsWith(gxxIncludeDirKey))
                            {
                                gxxIncludeDir = option.Substring(gxxIncludeDirKey.Length).Trim();
                            }
                            else if (option.StartsWith(targetKey))
                            {
                                ComposerXETarget = option.Substring(targetKey.Length).Trim();
                            }
                        }

                        break;
                    }
                }

                if (null == ComposerXETarget)
                {
                    foreach (string detail in splitDetails)
                    {
                        string targetKey = "Target: ";
                        if (detail.StartsWith(targetKey))
                        {
                            ComposerXETarget = detail.Substring(targetKey.Length).Trim();
                        }
                    }
                }

                if (!gxxIncludeDir.StartsWith(pathPrefix))
                {
                    // remove any prefix directory separator so that Combine works
                    gxxIncludeDir = gxxIncludeDir.TrimStart(System.IO.Path.DirectorySeparatorChar);
                    gxxIncludeDir = System.IO.Path.Combine(pathPrefix, gxxIncludeDir);
                }
            }

            ComposerXEDetails ComposerXEDetails = new ComposerXEDetails(ComposerXEVersion, gxxIncludeDir, ComposerXETarget);
            ComposerXEDetailsForTarget[target] = ComposerXEDetails;

            Opus.Core.Log.DebugMessage("ComposerXE version for target '{0}' is '{1}'", target.ToString(), ComposerXEDetails.Version);
            Opus.Core.Log.DebugMessage("ComposerXE machine type for target '{0}' is '{1}'", target.ToString(), ComposerXEDetails.Target);
            Opus.Core.Log.DebugMessage("Gxx include path for target '{0}' is '{1}'", target.ToString(), ComposerXEDetails.GxxIncludePath);
        }

        public string ComposerXEVersion(Opus.Core.Target target)
        {
            if (!ComposerXEDetailsForTarget.ContainsKey(target))
            {
                GetComposerXEDetails(target);
            }

            return ComposerXEDetailsForTarget[target].Version;
        }

        public string MachineType(Opus.Core.Target target)
        {
            if (!ComposerXEDetailsForTarget.ContainsKey(target))
            {
                GetComposerXEDetails(target);
            }

            return ComposerXEDetailsForTarget[target].Target;
        }

        public string GxxIncludePath(Opus.Core.Target target)
        {
            if (!ComposerXEDetailsForTarget.ContainsKey(target))
            {
                GetComposerXEDetails(target);
            }

            return ComposerXEDetailsForTarget[target].GxxIncludePath;
        }
    }
#endif
}

