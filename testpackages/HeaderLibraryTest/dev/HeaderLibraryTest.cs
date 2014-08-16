// Automatically generated by Opus v0.50
namespace HeaderLibraryTest
{
    class HeaderLibrary :
        C.HeaderLibrary
    {
        public
        HeaderLibrary()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(HeaderLibrary_IncludePaths);
        }

        [C.ExportCompilerOptionsDelegate]
        void
        HeaderLibrary_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            options.IncludePaths.Include(this.PackageLocation, "include");
        }

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();
    }

    class Application :
        C.Application
    {
        public
        Application()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "main.c");
        }

        [Bam.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(HeaderLibrary)
            );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );
    }
}
