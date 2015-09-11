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
    public sealed partial class NativeBuilder :
        Bam.Core.IBuilder
    {
        static
        NativeBuilder()
        {
            var level = Bam.Core.EVerboseLevel.Full;
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "Explain"))
                {
                    level = Bam.Core.State.VerbosityLevel;
                }
            }
            Verbosity = level;
        }

        private static Bam.Core.EVerboseLevel Verbosity
        {
            get;
            set;
        }

        public static void
        MakeDirectory(
            string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                Bam.Core.Log.Message(Verbosity, "Created directory '{0}'", directory);
            }
        }

        public static bool
        RequiresBuilding(
            string outputPath,
            string inputPath)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            var inputFileDate = System.IO.File.GetLastWriteTime(inputPath);
            if (System.IO.File.Exists(outputPath))
            {
                var outputFileDate = System.IO.File.GetLastWriteTime(outputPath);
                if (inputFileDate.CompareTo(outputFileDate) > 0)
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{1}' since input file '{0}' is newer.", inputPath, outputPath);
                    return true;
                }
            }
            else
            {
                Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputPath);
                return true;
            }

            return false;
        }

        public static bool
        DirectoryUpToDate(
            string destinationDir,
            string sourceDir)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return false;
                }
            }

            var inputDirDate = System.IO.Directory.GetLastWriteTime(sourceDir);
            if (System.IO.Directory.Exists(destinationDir))
            {
                var outputDirDate = System.IO.Directory.GetLastWriteTime(destinationDir);
                if (inputDirDate.CompareTo(outputDirDate) > 0)
                {
                    Bam.Core.Log.Message(Verbosity, "Building directory '{1}' since source directory '{0}' is newer.", sourceDir, destinationDir);
                    return false;
                }
            }
            else
            {
                Bam.Core.Log.Message(Verbosity, "Building directory '{0}' since it does not exist.", destinationDir);
                return false;
            }

            return true;
        }

        public static bool
        DirectoryUpToDate(
            Bam.Core.Location destinationDir,
            string sourceDir)
        {
            var destinationDirPath = destinationDir.GetSinglePath();
            return DirectoryUpToDate(destinationDirPath, sourceDir);
        }

        public enum FileRebuildStatus
        {
            AlwaysBuild,
            TimeStampOutOfDate,
            UpToDate
        }

        public static FileRebuildStatus
        IsSourceTimeStampNewer(
            Bam.Core.StringArray outputFiles,
            string inputFile)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return FileRebuildStatus.AlwaysBuild;
            }

            var newestInputFileDate = System.IO.File.GetLastWriteTime(inputFile);

            foreach (var outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer.", inputFile, outputFile);
                        return FileRebuildStatus.TimeStampOutOfDate;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            return FileRebuildStatus.UpToDate;
        }

        public static FileRebuildStatus
        IsSourceTimeStampNewer(
            Bam.Core.LocationArray outputFiles,
            Bam.Core.Location inputFile)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return FileRebuildStatus.AlwaysBuild;
            }

            var inputFilePath = inputFile.GetSinglePath();
            var newestInputFileDate = System.IO.File.GetLastWriteTime(inputFilePath);

            foreach (var outputFile in outputFiles)
            {
                var outputFilePath = outputFile.GetSinglePath();
                if (System.IO.File.Exists(outputFilePath))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFilePath);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer.", inputFilePath, outputFilePath);
                        return FileRebuildStatus.TimeStampOutOfDate;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFilePath);
                    return FileRebuildStatus.AlwaysBuild;
                }
            }

            return FileRebuildStatus.UpToDate;
        }

        // TODO: what if some of the paths passed in are directories? And what if they don't exist?
        public static bool
        RequiresBuilding(
            Bam.Core.StringArray outputFiles,
            Bam.Core.StringArray inputFiles)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No input files - always build");
                return true;
            }

            var newestInputFileDate = new System.DateTime(2000, 1, 1);
            string newestInputFile = null;
            foreach (var inputFile in inputFiles)
            {
                var inputFileLastWriteTime = System.IO.File.GetLastWriteTime(inputFile);
                if (inputFileLastWriteTime.CompareTo(newestInputFileDate) > 0)
                {
                    newestInputFileDate = inputFileLastWriteTime;
                    newestInputFile = inputFile;
                }
            }

            foreach (var outputFile in outputFiles)
            {
                if (System.IO.File.Exists(outputFile))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFile);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer", newestInputFile, outputFile);
                        return true;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFile);
                    return true;
                }
            }

            return false;
        }

        // TODO: what if some of the paths passed in are directories? And what if they don't exist?
        public static bool
        RequiresBuilding(
            Bam.Core.LocationArray outputFiles,
            Bam.Core.LocationArray inputFiles)
        {
            if (Bam.Core.State.HasCategory("NativeBuilder"))
            {
                if ((bool)Bam.Core.State.Get("NativeBuilder", "ForceBuild"))
                {
                    return true;
                }
            }

            if (0 == outputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No output files - always build");
                return true;
            }
            if (0 == inputFiles.Count)
            {
                Bam.Core.Log.Message(Verbosity, "No input files - always build");
                return true;
            }

            var newestInputFileDate = new System.DateTime(2000, 1, 1);
            string newestInputFile = null;
            foreach (var inputFile in inputFiles)
            {
                var inputFilePath = inputFile.GetSingleRawPath();
                var inputFileLastWriteTime = System.IO.File.GetLastWriteTime(inputFilePath);
                if (inputFileLastWriteTime.CompareTo(newestInputFileDate) > 0)
                {
                    newestInputFileDate = inputFileLastWriteTime;
                    newestInputFile = inputFilePath;
                }
            }

            foreach (var outputFile in outputFiles)
            {
                var outputFilePath = outputFile.GetSingleRawPath();
                if (System.IO.File.Exists(outputFilePath))
                {
                    var outputFileLastWriteTime = System.IO.File.GetLastWriteTime(outputFilePath);
                    if (newestInputFileDate.CompareTo(outputFileLastWriteTime) > 0)
                    {
                        Bam.Core.Log.Message(Verbosity, "Building '{1}' since '{0}' is newer", newestInputFile, outputFilePath);
                        return true;
                    }
                }
                else
                {
                    Bam.Core.Log.Message(Verbosity, "Building '{0}' since it does not exist.", outputFilePath);
                    return true;
                }
            }

            return false;
        }
    }
}