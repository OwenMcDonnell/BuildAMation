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
using Bam.Core;
namespace C
{
    /// <summary>
    /// Derive against this module to create a dynamic library, linking against the C runtime library.
    /// </summary>
    public class DynamicLibrary :
        ConsoleApplication,
        IDynamicLibrary,
        IForwardedLibraries
    {
        private Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();
        private SharedObjectSymbolicLink linkerNameSymLink = null;
        private SharedObjectSymbolicLink soNameSymLink = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths[Key] = this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(dynamicprefix)$(OutputName)$(dynamicext)");
            this.Macros.Add("LinkOutput", this.GeneratedPaths[Key]);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros.Add("ImportLibraryName", Bam.Core.TokenizedString.Create("$(OutputName)", null));
                this.RegisterGeneratedFile(ImportLibraryKey, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(libprefix)$(ImportLibraryName)$(libext)"));
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                if (!(this is Plugin) && !this.IsPrebuilt)
                {
                    // TODO: I wonder if these macros can be removed? (requires a change to GccCommon.ICommonLinkerSettings)
                    this.Macros.Add("SOName", this.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(sonameext)"));
                    this.Macros.Add("LinkerName", this.CreateTokenizedString("$(dynamicprefix)$(OutputName)$(linkernameext)"));

                    var linkerName = Bam.Core.Module.Create<SharedObjectSymbolicLink>(preInitCallback:module=>
                        {
                            module.Macros.AddVerbatim("SymlinkUsage", "LinkerName");
                            module.SharedObject = this;
                        });
                    this.LinkerNameSymbolicLink = linkerName;

                    var SOName = Bam.Core.Module.Create<SharedObjectSymbolicLink>(preInitCallback:module=>
                        {
                            module.Macros.AddVerbatim("SymlinkUsage", "SOName");
                            module.SharedObject = this;
                        });
                    this.SONameSymbolicLink = SOName;
                }
            }

            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerSettings;
                if (null != linker)
                {
                    linker.OutputType = ELinkerOutput.DynamicLibrary;
                }

                var osxLinker = settings as C.ICommonLinkerSettingsOSX;
                if (null != osxLinker)
                {
                    osxLinker.InstallName = this.CreateTokenizedString("@rpath/@filename($(LinkOutput))");
                }
            });
        }

        // TODO: what is this used for?
        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> Source
        {
            get
            {
                return this.sourceModules.ToReadOnlyCollection();
            }
        }

        public sealed override CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateCSourceContainer(wildcardPath, macroModuleOverride, filter);
            collection.PrivatePatch(settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
                (collection.Tool as C.CompilerTool).CompileAsShared(settings);
            });
            return collection;
        }

        public sealed override AssembledObjectFileCollection
        CreateAssemblerSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var collection = base.CreateAssemblerSourceContainer(wildcardPath, macroModuleOverride, filter);
            return collection;
        }

        /// <summary>
        /// Specified sources compile against DependentModule, and re-exports the public patches
        /// from the dependent, e.g. if the headers of the dynamic library require include paths from
        /// the dependent.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="additionalSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompileAgainstPublicly<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.DependsOn(dependent);
            var sources = new CModule[additionalSources.Length + 1];
            sources[0] = affectedSource;
            if (additionalSources.Length > 0)
            {
                additionalSources.CopyTo(sources, 1);
            }
            foreach (var source in sources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// Specified sources and the application compiles and links against the DependentModule, and the
        /// application uses patches from the dependent.
        /// </summary>
        /// <param name="affectedSource">Required source module.</param>
        /// <param name="additionalSources">Optional list of additional sources.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        CompilePubliclyAndLinkAgainst<DependentModule>(
            CModule affectedSource,
            params CModule[] additionalSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            if (dependent is C.DynamicLibrary || dependent is C.Cxx.DynamicLibrary)
            {
                this.forwardedDeps.AddUnique(dependent);
            }
            this.CompileAndLinkAgainst<DependentModule>(affectedSource, additionalSources);
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// DynamicLibrary is linked against the DependentModule type.
        /// Any public patches of DependentModule are applied publicly to the DynamicLibrary, and will be inherited
        /// by any module depending on the DynamicLibrary.
        /// </summary>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        LinkPubliclyAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.addLinkDependency(dependent);
            this.addRuntimeDependency(dependent);
            if (dependent is C.DynamicLibrary || dependent is C.Cxx.DynamicLibrary)
            {
                this.forwardedDeps.AddUnique(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
            this.UsePublicPatches(dependent);
        }

        /// <summary>
        /// Extend a container of C object files with another, potentially from another module. Note that module types must match.
        /// Private patches are inherited.
        /// Public patches are both used internally, and also forwarded onto any callers of this module.
        /// </summary>
        /// <typeparam name="DependentModule">Container module type to embed into the specified container.</typeparam>
        /// <param name="affectedSource">Container to be extended.</param>
        public void
        ExtendSourcePublicly<DependentModule>(
            CObjectFileCollection affectedSource) where DependentModule : CObjectFileCollection, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            affectedSource.ExtendWith(dependent);
            this.UsePublicPatches(dependent);
        }

        protected sealed override void
        ExecuteInternal(
            ExecutionContext context)
        {
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            base.ExecuteInternal(context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            if (this.IsPrebuilt &&
                !((this.headerModules.Count > 0) && Bam.Core.Graph.Instance.BuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles))
            {
                return;
            }
            base.GetExecutionPolicy(mode);
        }

        System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries
        {
            get
            {
                return this.forwardedDeps.ToReadOnlyCollection();
            }
        }

        public override TokenizedString WorkingDirectory
        {
            set
            {
                throw new System.NotSupportedException("Cannot set a working directory on a DLL");
            }
        }

        SharedObjectSymbolicLink IDynamicLibrary.LinkerNameSymbolicLink
        {
            get
            {
                return this.linkerNameSymLink;
            }
        }
        protected SharedObjectSymbolicLink LinkerNameSymbolicLink
        {
            set
            {
                this.linkerNameSymLink = value;
            }
        }

        SharedObjectSymbolicLink IDynamicLibrary.SONameSymbolicLink
        {
            get
            {
                return this.soNameSymLink;
            }
        }
        protected SharedObjectSymbolicLink SONameSymbolicLink
        {
            set
            {
                this.soNameSymLink = value;
            }
        }
    }
}
