// Automatically generated by Opus v0.50
namespace CocoaTest1
{
    [Opus.Core.ModuleTargets(Platform=Opus.Core.EPlatform.OSX)]
    class CocoaTest : C.WindowsApplication
    {
        public CocoaTest()
        {
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var link = module.Options as C.ILinkerOptionsOSX;
                link.ApplicationBundle = true;
                link.Frameworks.Add("Cocoa");
            };
        }

        class Source : C.ObjC.ObjectFileCollection
        {
            public Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.m");
            }
        }

        [Opus.Core.SourceFiles]
        Source source = new Source();
    }

    [Opus.Core.ModuleTargets(Platform=Opus.Core.EPlatform.OSX)]
    class CocoaTestPlist : XmlUtilities.OSXPlistModule
    {
        public CocoaTestPlist()
        {
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as XmlUtilities.IOSXPlistOptions;
                options.CFBundleName = "CocoaTest1";
                options.CFBundleDisplayName = "CocoaTest1";
                options.CFBundleIdentifier = "CocoaTest1";
                options.CFBundleVersion = "1.0.0";
            };
        }

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(CocoaTest)
            );
    }
}
