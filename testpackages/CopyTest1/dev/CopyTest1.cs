// Automatically generated by Opus v0.50
namespace CopyTest1
{
    class CopySingleFileTest : FileUtilities.CopyFile
    {
        public CopySingleFileTest()
        {
            this.SetRelativePath(this, "data", "testfile.txt");
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                FileUtilities.ICopyFileOptions options = module.Options as FileUtilities.ICopyFileOptions;
                if (null != options)
                {
                    if (target.HasPlatform(Opus.Core.EPlatform.OSX))
                    {
                        options.DestinationDirectory = "/tmp";
                    }
                    else if (target.HasPlatform(Opus.Core.EPlatform.Unix))
                    {
                        options.DestinationDirectory = "/tmp";
                    }
                    else if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                    {
                        options.DestinationDirectory = @"c:/temp";
                    }
                }
           };
        }
    }

    class CopyMultipleFileTest : FileUtilities.CopyFileCollection
    {
        public CopyMultipleFileTest()
        {
            this.Include(this, "data", "*");
        }
    }

    class CopyDirectoryTest : FileUtilities.CopyDirectory
    {
        public CopyDirectoryTest()
        {
            this.Include(this, "data");
        }
    }
}
