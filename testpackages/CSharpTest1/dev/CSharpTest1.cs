// Automatically generated by Opus v0.00
namespace CSharpTest1
{
    // Define module classes here
    class SimpleLibrary : CSharp.Library
    {
        public SimpleLibrary()
        {
            this.source.Include(this, "source", "simpletest.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.FileCollection source = new Opus.Core.FileCollection();
    }

    class SimpleExecutable : CSharp.Executable
    {
        public SimpleExecutable()
        {
            this.source.SetRelativePath(this, "source", "simpleexecutable.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
    }

    class SimpleWindowExecutable : CSharp.WindowsExecutable
    {
        public SimpleWindowExecutable()
        {
            this.source.SetRelativePath(this, "source", "simplewindowsexecutable.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
    }

    class SimpleModule : CSharp.Module
    {
        public SimpleModule()
        {
            this.source.SetRelativePath(this, "source", "simplemodule.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
    }

    class Executable2 : CSharp.Executable
    {
        public Executable2()
        {
            this.source.SetRelativePath(this, "source", "executable2.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(SimpleLibrary));
    }

    // TODO: There needs to be a better predication test here
#if true
    class PublishAssembliesForExecutable2 : FileUtilities.CopyFile
    {
        public PublishAssembliesForExecutable2()
        {
            this.Set(typeof(SimpleLibrary), CSharp.OutputFileFlags.AssemblyFile);
        }

        [FileUtilities.BesideModule(CSharp.OutputFileFlags.AssemblyFile)]
        System.Type nextTo = typeof(Executable2);
    }
#else
    class PublishAssembliesForExecutable2 : FileUtilities.CopyFiles
    {
        [FileUtilities.SourceModules(CSharp.OutputFileFlags.AssemblyFile)]
        Opus.Core.TypeArray sourceTargets = new Opus.Core.TypeArray(typeof(SimpleLibrary));

        [FileUtilities.DestinationModuleDirectory(CSharp.OutputFileFlags.AssemblyFile)]
        Opus.Core.TypeArray destinationTarget = new Opus.Core.TypeArray(typeof(Executable2));
    }
#endif

    class ExecutableReferences : CSharp.Executable
    {
        public ExecutableReferences()
        {
            this.source.SetRelativePath(this, "source", "executablexml.cs");

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(ExecutableReferences_UpdateOptions);
        }

        void ExecutableReferences_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            CSharp.IOptions options = module.Options as CSharp.IOptions;
            options.References.Add("System.Xml.dll");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
    }
}
