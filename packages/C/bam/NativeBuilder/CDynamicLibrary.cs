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
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.DynamicLibrary moduleToBuild,
            out bool success)
        {
            var dynamicLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;
            var dynamicLibraryOptions = dynamicLibraryModule.Options;
            var linkerOptions = dynamicLibraryOptions as C.ILinkerOptions;

            // find dependent object files
            var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.ObjectFile.OutputFile
                );
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                keysToFilter.Add(C.Win32Resource.OutputFile);
            }
            var dependentObjectFiles = new Bam.Core.LocationArray();
            if (null != node.Children)
            {
                node.Children.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (null != node.ExternalDependents)
            {
                node.ExternalDependents.FilterOutputLocations(keysToFilter, dependentObjectFiles);
            }
            if (0 == dependentObjectFiles.Count)
            {
                Bam.Core.Log.Detail("There were no object files to link for module '{0}'", node.UniqueModuleName);
                success = true;
                return null;
            }

            // find dependent library files
            Bam.Core.LocationArray dependentLibraryFiles = null;
            if (null != node.ExternalDependents)
            {
                dependentLibraryFiles = new Bam.Core.LocationArray();

                var libraryKeysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey);
                if (target.HasPlatform(Bam.Core.EPlatform.Linux))
                {
                    libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                node.ExternalDependents.FilterOutputLocations(libraryKeysToFilter, dependentLibraryFiles);
            }

            // dependency checking
            {
                var inputFiles = new Bam.Core.LocationArray();
                inputFiles.AddRange(dependentObjectFiles);
                if (null != dependentLibraryFiles)
                {
                    inputFiles.AddRange(dependentLibraryFiles);
                }

                var outputFiles = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                // don't dependency check against the static import library, since it is generally not rewritten
                // when code changes
                if (outputFiles.Contains(moduleToBuild.Locations[C.DynamicLibrary.ImportLibraryFile]))
                {
                    outputFiles.Remove(moduleToBuild.Locations[C.DynamicLibrary.ImportLibraryFile]);
                }
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Bam.Core.StringArray();
            if (linkerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = linkerOptions as CommandLineProcessor.ICommandLineSupport;
                // libraries are manually added later
                var excludedOptions = new Bam.Core.StringArray("Libraries", "StandardLibraries");
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);
            }
            else
            {
                throw new Bam.Core.Exception("Linker options does not support command line translation");
            }

            // object files must come before everything else, for some compilers
            commandLineBuilder.Insert(0, dependentObjectFiles.Stringify(" "));

            // then libraries
            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            C.LinkerUtilities.AppendLibrariesToCommandLine(commandLineBuilder, linkerTool, linkerOptions, dependentLibraryFiles);

            var exitCode = CommandLineProcessor.Processor.Execute(node, linkerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
