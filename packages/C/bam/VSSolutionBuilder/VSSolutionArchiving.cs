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
using System.Linq;
namespace C
{
#if BAM_V2
    public static partial class VSSolutionSupport
    {
        public static void
        Archive(
            StaticLibrary module)
        {
            VSSolutionBuilder.VSSolution solution;
            VSSolutionBuilder.VSProjectConfiguration config;

            LinkOrArchive(
                out solution,
                out config,
                module,
                VSSolutionBuilder.VSProjectConfiguration.EType.StaticLibrary,
                module.ObjectFiles,
                module.HeaderFiles
            );
            if (null == config)
            {
                return;
            }

            // now add the librarian settings
            var librarianGroup = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Librarian);
            VisualStudioProcessor.VSSolutionConversion.Convert(
                module.Settings,
                module.Settings.GetType(),
                module,
                librarianGroup
            );

            // order only dependents
            foreach (var proj in GetOrderOnlyDependentProjects(module))
            {
                config.RequiresProject(proj);
            }
        }
    }
#else
    public sealed class VSSolutionLibrarian :
        IArchivingPolicy
    {
        void
        IArchivingPolicy.Archive(
            StaticLibrary sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers)
        {
            if (0 == objectFiles.Count)
            {
                return;
            }

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(sender);
            var config = project.GetConfiguration(sender);

            config.SetType(VSSolutionBuilder.VSProjectConfiguration.EType.StaticLibrary);
            config.SetOutputPath(libraryPath);
            config.EnableIntermediatePath();

            foreach (var header in headers)
            {
                config.AddHeaderFile(header as HeaderFile);
            }

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Compiler);

            var realObjectFiles = objectFiles.Where(item => !((item is WinResource) || (item is AssembledObjectFile)));
            if (realObjectFiles.Any())
            {
                var vsConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
                    typeof(VSSolutionBuilder.VSSettingsGroup),
                    typeof(string)
                };

                var sharedSettings = C.SettingsBase.SharedSettings(
                    realObjectFiles,
                    typeof(VisualCCommon.VSSolutionImplementation),
                    typeof(VisualStudioProcessor.IConvertToProject),
                    vsConvertParameterTypes);
                (sharedSettings as VisualStudioProcessor.IConvertToProject).Convert(sender, compilerGroup);

                foreach (var objFile in realObjectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    config.AddSourceFile(objFile, deltaSettings);
                }
            }

            // now handle the other object file types

            // TODO: if there were many resource files, this could also have a common settings group? Not sure if VS supports this
            // and it's not as likely to have many resource files, as it would have many source files
            var resourceObjectFiles = objectFiles.Where(item => item is WinResource);
            foreach (var resObj in resourceObjectFiles)
            {
                config.AddResourceFile(resObj as WinResource, resObj.Settings);
            }

            var assembledObjectFiles = objectFiles.Where(item => item is AssembledObjectFile);
            foreach (var asmObj in assembledObjectFiles)
            {
                config.AddAssemblyFile(asmObj as AssembledObjectFile, asmObj.Settings);
            }

            var vsSettingsGroup = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Librarian);
            (sender.Settings as VisualStudioProcessor.IConvertToProject).Convert(sender, vsSettingsGroup);

            // order only dependencies
            foreach (var required in sender.Requirements)
            {
                if (null == required.MetaData)
                {
                    continue;
                }

                var requiredProject = required.MetaData as VSSolutionBuilder.VSProject;
                if (null != requiredProject)
                {
                    config.RequiresProject(requiredProject);
                }
            }
            // any non-C module projects should be order-only dependencies
            // note: this is unlikely to happen, as StaticLibraries don't have hard 'dependencies'
            // because there is no fixed 'link' action at the end
            foreach (var dependent in sender.Dependents)
            {
                if (null == dependent.MetaData)
                {
                    continue;
                }
                if (dependent is C.CModule)
                {
                    continue;
                }
                var dependentProject = dependent.MetaData as VSSolutionBuilder.VSProject;
                if (null != dependentProject)
                {
                    config.RequiresProject(dependentProject);
                }
            }
            // however, there may be forwarded libraries, and these are useful order only dependents
            foreach (var dependent in (sender as IForwardedLibraries).ForwardedLibraries)
            {
                if (null == dependent.MetaData)
                {
                    continue;
                }
                var dependentProject = dependent.MetaData as VSSolutionBuilder.VSProject;
                if (null != dependentProject)
                {
                    config.RequiresProject(dependentProject);
                }
            }
        }
    }
#endif
}
