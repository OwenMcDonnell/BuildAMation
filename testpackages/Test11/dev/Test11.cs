// Automatically generated by Opus v0.00
namespace Test11
{
    // Define module classes here
    class CrossPlatformApplication : C.Application
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        public CrossPlatformApplication()
        {
            this.commonSourceFile.SetRelativePath(this, "source", "main.c");
            this.winSourceFile.SetRelativePath(this, "source", "win", "win.c");
            this.unixSourceFile.SetRelativePath(this, "source", "unix", "unix.c");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile commonSourceFile = new C.ObjectFile();

        [Opus.Core.SourceFiles("win.*-.*-.*")]
        C.ObjectFile winSourceFile = new C.ObjectFile();

        [Opus.Core.SourceFiles("unix.*-.*-.*")]
        C.ObjectFile unixSourceFile = new C.ObjectFile();

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray WinVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray WinVCLibraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }
}
