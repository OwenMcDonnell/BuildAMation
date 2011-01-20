// Automatically generated by Opus v0.00
namespace CSharpTest1
{
    // Define module classes here
    class SimpleLibrary : CSharp.Library
    {
        public SimpleLibrary()
        {
            this.source.SetRelativePath(this, "source", "simpletest.cs");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File source = new Opus.Core.File();
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
}
