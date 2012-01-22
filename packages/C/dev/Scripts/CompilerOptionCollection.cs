// <copyright file="CompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class CompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        // TODO:  no reason why this can't be a static utility function
        protected virtual void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerOptions = this as ICCompilerOptions;

            compilerOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);

            Opus.Core.Target target = node.Target;

            Opus.Core.EConfiguration configuration = target.Configuration;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (Opus.Core.EConfiguration.Debug == configuration)
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
            }
            else
            {
                if (Opus.Core.EConfiguration.Profile != configuration)
                {
                    compilerOptions.DebugSymbols = false;
                }
                else
                {
                    compilerOptions.DebugSymbols = true;
                }
                compilerOptions.Optimization = EOptimization.Speed;
            }
            compilerOptions.CustomOptimization = "";
            compilerOptions.ShowIncludes = false;

            compilerOptions.Defines = new DefineCollection();
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_{0}", target.Platform.ToString().ToUpper()));
            {
                bool is64bit = Opus.Core.OSUtilities.Is64Bit(target.Platform);
                int bits = (is64bit) ? 64 : 32;
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_BITS={0}", bits.ToString()));
            }
            {
                bool isLittleEndian = Opus.Core.State.IsLittleEndian;
                if (isLittleEndian)
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_BIGENDIAN");
                }
            }
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_CONFIGURATION_{0}", target.Configuration.ToString().ToUpper()));
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_TOOLCHAIN_{0}", target.Toolchain.ToUpper()));

            compilerOptions.IncludePaths = new Opus.Core.DirectoryCollection();
            compilerOptions.IncludePaths.AddAbsoluteDirectory(".", true); // explicitly add the one that is assumed

            compilerOptions.SystemIncludePaths = new Opus.Core.DirectoryCollection();

            compilerOptions.DisableWarnings = new Opus.Core.StringArray();

            compilerOptions.AdditionalOptions = "";
        }

        public CompilerOptionCollection()
            : base()
        {
        }

        public CompilerOptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetNodeOwnership(node);
            this.InitializeDefaults(node);
            this.SetDelegates(node);
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            ObjectFile objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                string sourcePathName = (node.Module as ObjectFile).SourceFile.AbsolutePath;
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }
            else
            {
                this.OutputName = null;
            }

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.ObjectFileOutputSubDirectory);
        }

        public string OutputName
        {
            get;
            set;
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public abstract string ObjectFilePath
        {
            get;
            set;
        }

        public abstract string PreprocessedFilePath
        {
            get;
            set;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}