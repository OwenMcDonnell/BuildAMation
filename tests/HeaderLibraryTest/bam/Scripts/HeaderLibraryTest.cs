#region License
// Copyright (c) 2010-2015, Mark Final
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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace HeaderLibraryTest
{
    public sealed class HeaderLibraryV2 :
        C.V2.HeaderLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/unusedmacros.h");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                    }
                });
        }
    }

    public sealed class ApplicationV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/main.c");

            this.CompileAgainst<HeaderLibraryV2>(source);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    class HeaderLibrary :
        C.HeaderLibrary
    {
        public
        HeaderLibrary()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(HeaderLibrary_IncludePaths);
        }

        [C.ExportCompilerOptionsDelegate]
        void
        HeaderLibrary_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            options.IncludePaths.Include(this.PackageLocation, "include");
        }

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();
    }

    class Application :
        C.Application
    {
        public
        Application()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "main.c");
        }

        [Bam.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(HeaderLibrary)
            );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );
    }
}