#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace Qt
{
    public sealed class Toolset :
        QtCommon.Toolset
    {
        private string installPath = null;

        protected override string
        GetInstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return this.installPath;
            }

            if (Bam.Core.State.HasCategory("Qt") && Bam.Core.State.Has("Qt", "InstallPath"))
            {
                this.installPath = Bam.Core.State.Get("Qt", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("Qt install path set from command line to '{0}'", this.installPath);
                return this.installPath;
            }

            string installPath = null;
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                using (Microsoft.Win32.RegistryKey key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\Qt 5.2.0"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Qt libraries for 5.2.0 were not installed");
                    }

                    installPath = key.GetValue("InstallLocation") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("Unable to locate InstallLocation registry key for Qt 5.2.0");
                    }

                    // precompiled binaries now have a subdirectory indicating their flavour
                    installPath += @"\5.2.0\msvc2010_opengl";

                    Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                }
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.2.0");
                    installPath = System.IO.Path.Combine(installPath, "5.2.0");
                    installPath = System.IO.Path.Combine(installPath, "gcc_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.2.0/5.2.0/gcc_64";
                }
            }
            else if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null != homeDir)
                {
                    installPath = System.IO.Path.Combine(homeDir, "Qt5.2.0");
                    installPath = System.IO.Path.Combine(installPath, "5.2.0");
                    installPath = System.IO.Path.Combine(installPath, "clang_64");
                }
                else
                {
                    installPath = @"/usr/local/Qt-5.2.0/5.2.0/clang_64";
                }
            }
            else
            {
                throw new Bam.Core.Exception("Qt identification has not been implemented on the current platform");
            }
            this.installPath = installPath;

            return installPath;
        }

        protected override string
        GetVersionNumber()
        {
            return "5.2.0";
        }
    }
}
