#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
    public sealed class VSSolutionLibrarian :
        ILibrarianPolicy
    {
        void
        ILibrarianPolicy.Archive(
            StaticLibrary sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs)
        {
            // cannot tell the architecture from the Librarian tool, so look at all the inputs
            // these should be consistent
            VSSolutionBuilder.V2.VSProjectMeta.EPlatform? platform = null;
            foreach (var input in inputs)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        var obj = child as C.V2.ObjectFile;
                        var thisPlatform = (obj.Compiler is VisualC.Compiler64 || obj.Compiler is VisualC.CxxCompiler64) ? VSSolutionBuilder.V2.VSProjectMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSProjectMeta.EPlatform.ThirtyTwo;
                        if (null == platform)
                        {
                            platform = thisPlatform;
                        }
                        else if (platform != thisPlatform)
                        {
                            throw new Bam.Core.Exception("Inconsistent object file architectures");
                        }
                    }
                }
                else
                {
                    var obj = input as C.V2.ObjectFile;
                    var thisPlatform = (obj.Compiler is VisualC.Compiler64 || obj.Compiler is VisualC.CxxCompiler64) ? VSSolutionBuilder.V2.VSProjectMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSProjectMeta.EPlatform.ThirtyTwo;
                    if (null == platform)
                    {
                        platform = thisPlatform;
                    }
                    else if (platform != thisPlatform)
                    {
                        throw new Bam.Core.Exception("Inconsistent object file architectures");
                    }
                }
            }

            var library = new VSSolutionBuilder.V2.VSProjectStaticLibrary(sender, libraryPath, platform.Value);
            var commonObject = inputs[0];
            library.SetCommonCompilationOptions(commonObject, commonObject.Settings);

            foreach (var input in inputs)
            {
                C.V2.SettingsBase deltaSettings = null;
                if (input != commonObject)
                {
                    deltaSettings = (input.Settings as C.V2.SettingsBase).Delta(commonObject.Settings, input);
                }

                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        C.V2.SettingsBase patchSettings = deltaSettings;
                        if (child.HasPatches)
                        {
                            if (null == patchSettings)
                            {
                                patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as C.V2.SettingsBase;
                            }
                            else
                            {
                                patchSettings = deltaSettings.Clone(child);
                            }
                            child.ApplySettingsPatches(patchSettings, honourParents: false);
                        }
                        library.AddObjectFile(child, patchSettings);
                    }
                }
                else
                {
                    library.AddObjectFile(input, deltaSettings);
                }
            }
        }
    }
}
}
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var staticLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;
            var moduleName = node.ModuleName;

            IProject projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                if (this.solutionFile.ProjectDictionary.ContainsKey(moduleName))
                {
                    projectData = this.solutionFile.ProjectDictionary[moduleName];
                }
                else
                {
                    success = true;
                    return null;
                }
            }

            {
                var platformName = VSSolutionBuilder.GetPlatformNameFromTarget(target);
                if (!projectData.Platforms.Contains(platformName))
                {
                    projectData.Platforms.Add(platformName);
                }
            }

            // solution folder
            {
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.IArchiverOptions archiverOptions = staticLibraryOptions as C.IArchiverOptions;
                    C.IToolchainOptions toolchainOptions = archiverOptions.ToolchainOptionCollection as C.IToolchainOptions;
                    EProjectCharacterSet characterSet;
                    switch (toolchainOptions.CharacterSet)
                    {
                        case C.ECharacterSet.NotSet:
                            characterSet = EProjectCharacterSet.NotSet;
                            break;

                        case C.ECharacterSet.Unicode:
                            characterSet = EProjectCharacterSet.UniCode;
                            break;

                        case C.ECharacterSet.MultiByte:
                            characterSet = EProjectCharacterSet.MultiByte;
                            break;

                        default:
                            characterSet = EProjectCharacterSet.Undefined;
                            break;
                    }
                    configuration.CharacterSet = characterSet;
#endif
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget((Bam.Core.BaseTarget)target, configuration);
                }
            }

            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    // TODO: change to var
                    foreach (Bam.Core.Location location in headerFileCollection)
                    {
                        var headerPath = location.GetSinglePath();
                        var cProject = projectData as ICProject;
                        lock (cProject.HeaderFiles)
                        {
                            if (!cProject.HeaderFiles.Contains(headerPath))
                            {
                                var headerFile = new ProjectFile(headerPath);
                                cProject.HeaderFiles.Add(headerFile);
                            }
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.StaticLibrary;

            var toolName = "VCLibrarianTool";
            var vcCLLibrarianTool = configuration.GetTool(toolName);
            if (null == vcCLLibrarianTool)
            {
                vcCLLibrarianTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLibrarianTool);

                var archiverOptions = staticLibraryOptions as C.ArchiverOptionCollection;
                configuration.OutputDirectory = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey];
                configuration.TargetName = archiverOptions.OutputName;

                if (staticLibraryOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = staticLibraryOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLLibrarianTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Archiver options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
