// Automatically generated by Opus v0.50
namespace Test15
{
    public class StaticLibrary1 :
        C.StaticLibrary
    {
        public
        StaticLibrary1()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            [C.ExportCompilerOptionsDelegate]
            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var cOptions = module.Options as C.ICCompilerOptions;
                if (null != cOptions)
                {
                    cOptions.IncludePaths.Include(this.PackageLocation, "include");
                }
            }
        }

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();
    }
}
