#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License

namespace Bam.Core
{
    /// <summary>
    /// Static utility class for configuring and querying OS specific details.
    /// </summary>
    public static class OSUtilities
    {
        private static System.Collections.Generic.Dictionary<string, StringArray> InstallLocationCache = new System.Collections.Generic.Dictionary<string, StringArray>();

        private static bool CheckFor64BitOS
        {
            get
            {
                if (Graph.Instance.ProcessState.RunningMono)
                {
                    // TODO: System.Environment.GetEnvironmentVariable("HOSTTYPE") returns null instead of something like "x86_64"
                    // TODO: this is a hack and a big assumption that you're not running a 32-bit OS on a 64-bit processor
                    var is64Bit = (8 == System.IntPtr.Size);
                    return is64Bit;
                }
                else
                {
                    // cannot do a check for the Wow6432Node as it does exist on some 32-bit Windows OS (Vista for example)
                    var is64Bit = (System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null);
                    return is64Bit;
                }
            }
        }

        // based on http://go-mono.com/forums/#nabble-td1549244
        private static class Platform
        {
            [System.Runtime.InteropServices.DllImport("libc")]
            static extern int uname(System.IntPtr buf);

            static private bool mIsWindows;
            static private bool mIsMac;

            public enum OS
            {
                Windows,
                OSX,
                Linux,
                unknown
            };

            static public OS
            GetOS()
            {
                if (mIsWindows = (System.IO.Path.DirectorySeparatorChar == '\\'))
                {
                    return OS.Windows;
                }

                if (mIsMac = (!mIsWindows && IsRunningOnMac()))
                {
                    return OS.OSX;
                }

                if (!mIsMac && System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                {
                    return OS.Linux;
                }

                return OS.unknown;
            }

            //From Managed.Windows.Forms/XplatUI
            static bool
            IsRunningOnMac()
            {
                var buf = System.IntPtr.Zero;
                try
                {
                    buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
                    // This is a hacktastic way of getting sysname from uname ()
                    if (uname(buf) == 0)
                    {
                        var os = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
                        if ("Darwin" == os)
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (buf != System.IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Configure the current platform, based on .NET framework settings.
        /// </summary>
        public static void
        SetupPlatform()
        {
            var os = Platform.GetOS();
            switch (os)
            {
                case Platform.OS.Windows:
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.Win64 : EPlatform.Win32;
                    break;

                case Platform.OS.Linux:
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.Linux64 : EPlatform.Linux32;
                    break;

                case Platform.OS.OSX:
                    CurrentPlatform = CheckFor64BitOS ? EPlatform.OSX64 : EPlatform.OSX32;
                    break;

                default:
                    throw new Exception("Unrecognized platform, {0}", os.ToString());
            }

            IsLittleEndian = System.BitConverter.IsLittleEndian;
        }

        /// <summary>
        /// Determines if is Windows the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is windows the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsWindows(
            EPlatform platform)
        {
            var isWindows = (EPlatform.Win32 == platform || EPlatform.Win64 == platform);
            return isWindows;
        }

        /// <summary>
        /// Determines if Windows is the current platform.
        /// </summary>
        /// <value><c>true</c> if is windows hosting; otherwise, <c>false</c>.</value>
        public static bool IsWindowsHosting
        {
            get
            {
                return IsWindows(CurrentPlatform);
            }
        }

        /// <summary>
        /// Determines if is Linux the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is linux the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsLinux(
            EPlatform platform)
        {
            var isLinux = (EPlatform.Linux32 == platform || EPlatform.Linux64 == platform);
            return isLinux;
        }

        /// <summary>
        /// Determines if Linux is the current platform.
        /// </summary>
        /// <value><c>true</c> if is linux hosting; otherwise, <c>false</c>.</value>
        public static bool IsLinuxHosting
        {
            get
            {
                return IsLinux(CurrentPlatform);
            }
        }

        /// <summary>
        /// Determines if is OSX the specified platform.
        /// </summary>
        /// <returns><c>true</c> if is OS the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        IsOSX(
            EPlatform platform)
        {
            var isOSX = (EPlatform.OSX32 == platform || EPlatform.OSX64 == platform);
            return isOSX;
        }

        /// <summary>
        /// Determines if OSX is the current platform.
        /// </summary>
        /// <value><c>true</c> if is OSX hosting; otherwise, <c>false</c>.</value>
        public static bool IsOSXHosting
        {
            get
            {
                return IsOSX(CurrentPlatform);
            }
        }

        /// <summary>
        /// Determines if the current platform is 64-bits.
        /// </summary>
        /// <returns><c>true</c> if is64 bit the specified platform; otherwise, <c>false</c>.</returns>
        /// <param name="platform">Platform.</param>
        public static bool
        Is64Bit(
            EPlatform platform)
        {
            var is64Bit = (EPlatform.Win64 == platform || EPlatform.Linux64 == platform || EPlatform.OSX64 == platform);
            return is64Bit;
        }

        /// <summary>
        /// Determines if the current OS is 64-bit.
        /// </summary>
        /// <value><c>true</c> if is64 bit hosting; otherwise, <c>false</c>.</value>
        public static bool Is64BitHosting
        {
            get
            {
                return Is64Bit(CurrentPlatform);
            }
        }

        /// <summary>
        /// Determines if the specified platform is supported by the current platform.
        /// </summary>
        /// <returns><c>true</c> if is current platform supported the specified supportedPlatforms; otherwise, <c>false</c>.</returns>
        /// <param name="supportedPlatforms">Supported platforms.</param>
        public static bool
        IsCurrentPlatformSupported(
            EPlatform supportedPlatforms)
        {
            var isSupported = (CurrentPlatform == (supportedPlatforms & CurrentPlatform));
            return isSupported;
        }

        /// <summary>
        /// Get the current platform in terms of the EPlatform enumeration.
        /// </summary>
        /// <value>The current O.</value>
        public static EPlatform
        CurrentOS
        {
            get
            {
                var os = Platform.GetOS();
                switch (os)
                {
                    case Platform.OS.Windows:
                        return EPlatform.Windows;
                    case Platform.OS.Linux:
                        return EPlatform.Linux;
                    case Platform.OS.OSX:
                        return EPlatform.OSX;
                    default:
                        throw new Exception("Unknown platform");
                }
            }
        }

        /// <summary>
        /// Determine if the current platform is little endian.
        /// </summary>
        /// <value><c>true</c> if is little endian; otherwise, <c>false</c>.</value>
        public static bool
        IsLittleEndian
        {
            get;
            private set;
        }

        /// <summary>
        /// Retrieve the current platform.
        /// </summary>
        /// <value>The current platform.</value>
        public static EPlatform CurrentPlatform
        {
            get;
            private set;
        }

        /// <summary>
        /// Retrieve the path to 'Program Files'. This is the path where native architecture programs are installed.
        /// The same path is returned on both 32-bit and 64-bit editions of Windows.
        /// </summary>
        public static TokenizedString WindowsProgramFilesPath
        {
            get
            {
                if (!IsWindowsHosting)
                {
                    throw new Exception("Only available on Windows");
                }
                return TokenizedString.CreateVerbatim(System.Environment.GetEnvironmentVariable("ProgramFiles"));
            }
        }

        /// <summary>
        /// Retrive the path to 'Program Files (x86)'. This is the path where 32-bit architecture programs are installed.
        /// On 32-bit Windows, this is the same as WindowsProgramFilesPath.
        /// On 64-bit Windows, it is a different path.
        /// </summary>
        public static TokenizedString WindowsProgramFilesx86Path
        {
            get
            {
                if (!IsWindowsHosting)
                {
                    throw new Exception("Only available on Windows");
                }
                var envVar = Is64BitHosting ? System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") : System.Environment.GetEnvironmentVariable("ProgramFiles");
                return TokenizedString.CreateVerbatim(envVar);
            }
        }

        /// <summary>
        /// Runs an executable, and returns a string containing the standard output
        /// from that run.
        /// </summary>
        /// <returns>Standard output from executable, or null in the case of failure.</returns>
        /// <param name="executable">Absolute path to executable to run.</param>
        /// <param name="arguments">All arguments to be passed to the executable.</param>
        public static string
        RunExecutable(
            string executable,
            string arguments)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = executable;
            processStartInfo.Arguments = arguments;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true; // swallow
            processStartInfo.UseShellExecute = false;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return null;
            }
            return process.StandardOutput.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
        }

        /// <summary>
        /// Gets the install location of an executable.
        /// The PATH is searched initially.
        /// If not found, on Windows, the x64 (if applicable) and x86 program files directories are recursively
        /// searched, in that order for the executable. This may be slow.
        /// If the searchDirectory argument is non-null, this is used instead of the Windows program file
        /// directories.
        /// An exception is thrown if it cannot be located in the system.
        /// Executable locations are cached (thread safe), so that multiple queries for the same
        /// executable does not need to invoke any external processes. If uniqueName is non-null, then
        /// this is used as the key in the cache, otherwise the executable name is used. The unique name
        /// may be useful to save different flavours of the same executable name.
        /// </summary>
        /// <returns>The installed locations of the executable (may be more than one). Or null if throwOnFailure is false when no match is found.</returns>
        /// <param name="executable">Filename of the executable to locate.</param>
        /// <param name="searchDirectory">Optional directory to search (Windows only).</param>
        /// <param name="uniqueName">Optional unique name to save as the key in the cache.</param>
        /// <param name="throwOnFailure">Optional Boolean, defaults to true, to indicate whether an exception is thrown when the executable is not found.</param>
        public static StringArray
        GetInstallLocation(
            string executable,
            string searchDirectory = null,
            string uniqueName = null,
            bool throwOnFailure = true)
        {
            lock (InstallLocationCache)
            {
                var key = (null != uniqueName) ? uniqueName : executable;
                if (InstallLocationCache.ContainsKey(key))
                {
                    return InstallLocationCache[key];
                }
                string location;
                if (OSUtilities.IsWindowsHosting)
                {
                    if (null != searchDirectory)
                    {
                        var args = new System.Text.StringBuilder();
                        args.AppendFormat("/R \"{0}\" {1}", searchDirectory, executable);
                        location = RunExecutable("where", args.ToString());
                    }
                    else
                    {
                        location = RunExecutable("where", executable);
                        if (null == location)
                        {
                            var args = new System.Text.StringBuilder();
                            args.AppendFormat("/R \"{0}\" {1}", WindowsProgramFilesPath.ToString(), executable);
                            location = RunExecutable("where", args.ToString());
                            if (null == location)
                            {
                                args.Length = 0;
                                args.Capacity = 0;
                                args.AppendFormat("/R \"{0}\" {1}", WindowsProgramFilesx86Path.ToString(), executable);
                                location = RunExecutable("where", args.ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (null != searchDirectory)
                    {
                        Log.DebugMessage("Search path '{0}' is ignored on non-Windows platforms", searchDirectory);
                    }
                    location = RunExecutable("which", executable);
                }
                if (null == location)
                {
                    if (throwOnFailure)
                    {
                        throw new Exception("Unable to locate '{0}' in the system.", executable);
                    }
                    else
                    {
                        return null;
                    }
                }
                var results = new StringArray(
                    location.Split(
                        new[] { System.Environment.NewLine },
                        System.StringSplitOptions.RemoveEmptyEntries
                    )
                );
                InstallLocationCache.Add(key, results);
                return results;
            }
        }
    }
}
