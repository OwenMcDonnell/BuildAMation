// <copyright file="ILinkerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ILinkerOptions
    {
#if false
        C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get;
            set;
        }
#endif

        C.ELinkerOutput OutputType
        {
            get;
            set;
        }

        bool DoNotAutoIncludeStandardLibraries
        {
            get;
            set;
        }

        bool DebugSymbols
        {
            get;
            set;
        }

        C.ESubsystem SubSystem
        {
            get;
            set;
        }

        bool DynamicLibrary
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection LibraryPaths
        {
            get;
            set;
        }

        Opus.Core.FileCollection StandardLibraries
        {
            get;
            set;
        }

        Opus.Core.FileCollection Libraries
        {
            get;
            set;
        }

        bool GenerateMapFile
        {
            get;
            set;
        }

        string AdditionalOptions
        {
            get;
            set;
        }
    }
}