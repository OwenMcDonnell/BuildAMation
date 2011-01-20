// Automatically generated by Opus v0.00
namespace Test6
{
    // Define module classes here
    class ConditionalApplication : C.Application
    {
        private const string WinVCTarget = "win.*-.*-visualc";

        // TODO: derive C.SourceFiles from this attribute?
        [Opus.Core.SourceFiles]
        private SourceFiles sourceFiles;

        public ConditionalApplication(Opus.Core.Target target)
        {
            this.sourceFiles = new SourceFiles(target);
            this.UpdateOptions += this.OverrideOptionCollection;
        }

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles(Opus.Core.Target target)
            {
                this.UpdateOptions += this.OverrideOptionCollection;

                C.ObjectFile mainObjectFile = new C.ObjectFile();
                mainObjectFile.SetRelativePath(this, "source", "main.c");
                mainObjectFile.UpdateOptions += MainUpdateOptionCollection;
                this.Add(mainObjectFile);

                if (Opus.Core.EConfiguration.Debug == target.Configuration)
                {
                    this.AddRelativePaths(this, "source", "debug", "debug.c");
                }
                else
                {
                    this.AddRelativePaths(this, "source", "optimized", "optimized.c");
                }
            }

            private void OverrideOptionCollection(Opus.Core.IModule module, Opus.Core.Target target)
            {
                C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test6"], @"include");
            }
        }

        private void OverrideOptionCollection(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            //options.DebugSymbols = false;
        }

        private static void MainUpdateOptionCollection(Opus.Core.IModule module, Opus.Core.Target target)
        {
            C.ICCompilerOptions compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.Defines.Add("MAIN_C");
            compilerOptions.IncludePaths.Add(Opus.Core.State.PackageInfo["Test6"], @"include/platform");
        }

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray(
            "KERNEL32.lib"
        );
    }
}
