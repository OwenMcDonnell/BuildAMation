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
namespace VisualC
{
    public class MetaData :
        VisualCCommon.MetaData,
        VisualCCommon.IRuntimeLibraryPathMeta
    {
        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            this.SolutionFormatVersion = "12.00";
            this.PlatformToolset = "v140";
            this.VCXProjToolsVersion = "14.0";
            this.VCXProjFiltersToolsVersion = "4.0";
        }

        protected override int major_version
        {
            get
            {
                return 14;
            }
        }

        protected override string subpath_to_vcvars
        {
            get
            {
                return "VC";
            }
        }

        public override object this[string index]
        {
            get
            {
                return this.Meta[index];
            }
        }

        public override bool
        Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        public string
        SolutionFormatVersion
        {
            get
            {
                return this.Meta["SolutionFormatVersion"] as string;
            }

            private set
            {
                this.Meta["SolutionFormatVersion"] = value;
            }
        }

        public string
        PlatformToolset
        {
            get
            {
                return this.Meta["PlatformToolset"] as string;
            }

            set
            {
                this.Meta["PlatformToolset"] = value;
            }
        }

        public string
        VCXProjToolsVersion
        {
            get
            {
                return this.Meta["VCXProjToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjToolsVersion"] = value;
            }
        }

        public string
        VCXProjFiltersToolsVersion
        {
            get
            {
                return this.Meta["VCXProjFiltersToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjFiltersToolsVersion"] = value;
            }
        }

        public int
        CompilerMajorVersion
        {
            get
            {
                return 19;
            }
        }

        public int
        CompilerMinorVersion
        {
            get
            {
                return 0;
            }
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CRuntimePaths(
            C.EBit depth)
        {
            // only redist the VisualC specific version runtime, and the universal CRT
            // vcvarsall.bat defines UniversalCRTSdkDir which is suitable for use with both WinSDK8.1 and 10.x
            // don't redist the api-ms-win-crt-*-l1-1-0.dll files from the WindowsSDK, as I can find no reference
            // to needing to do so

            var winsdkdir = this.Environment(depth)["UniversalCRTSdkDir"];
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    {
                        dynamicLibPaths.Add(
                            Bam.Core.TokenizedString.Create(
                                "$(0)/VC/redist/x86/Microsoft.VC140.CRT/vcruntime140.dll",
                                null,
                                new Bam.Core.TokenizedStringArray(this.InstallDir)
                            )
                        );
                        dynamicLibPaths.Add(
                            Bam.Core.TokenizedString.Create(
                                "$(0)/Redist/ucrt/DLLs/x86/ucrtbase.dll",
                                null,
                                new Bam.Core.TokenizedStringArray(winsdkdir)
                            )
                        );
                    }
                    break;

                case C.EBit.SixtyFour:
                    {
                        dynamicLibPaths.Add(
                            Bam.Core.TokenizedString.Create(
                                "$(0)/VC/redist/x64/Microsoft.VC140.CRT/vcruntime140.dll",
                                null,
                                new Bam.Core.TokenizedStringArray(this.InstallDir)
                            )
                        );
                        dynamicLibPaths.Add(
                            Bam.Core.TokenizedString.Create(
                                "$(0)/Redist/ucrt/DLLs/x64/ucrtbase.dll",
                                null,
                                new Bam.Core.TokenizedStringArray(winsdkdir)
                            )
                        );
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CxxRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(
                        Bam.Core.TokenizedString.Create(
                            "$(0)/VC/redist/x86/Microsoft.VC140.CRT/msvcp140.dll",
                            null,
                            new Bam.Core.TokenizedStringArray(this.InstallDir)
                        )
                    );
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(
                        Bam.Core.TokenizedString.Create(
                            "$(0)/VC/redist/x64/Microsoft.VC140.CRT/msvcp140.dll",
                            null,
                            new Bam.Core.TokenizedStringArray(this.InstallDir)
                        )
                    );
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }
    }
}
