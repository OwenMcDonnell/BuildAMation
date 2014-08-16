namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object
        Build(
            CodeGenTest2.CodeGenModule moduleToBuild,
            out System.Boolean success)
        {
            var codeGenModuleModule = moduleToBuild as Bam.Core.BaseModule;
            var node = codeGenModuleModule.OwningNode;
            var target = node.Target;
            var codeGenModuleOptions = codeGenModuleModule.Options;
            var toolOptions = codeGenModuleOptions as CodeGenTest2.CodeGenOptionCollection;
            var tool = target.Toolset.Tool(typeof(CodeGenTest2.ICodeGenTool));
            var toolExePath = tool.Executable((Bam.Core.BaseTarget)target);

            var inputFiles = new Bam.Core.StringArray();
            inputFiles.Add(toolExePath);

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Bam.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Moc options does not support command line translation");
            }

            var recipes = new Bam.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                recipes.Add("\"" + toolExePath + "\" " + commandLineBuilder.ToString());
            }
            else
            {
                recipes.Add(toolExePath + " " + commandLineBuilder.ToString());
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Bam.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                CodeGenTest2.CodeGenModule.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);
            rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(CodeGenTest2.CodeGenModule.OutputFile);
            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (tool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (tool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}
