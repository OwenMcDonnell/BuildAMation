// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            // any source file generated by moc should not be included in the .pro file (QMake handles this)
            string sourceFilePath = objectFile.SourceFile.AbsolutePath;
            if (System.IO.Path.GetFileNameWithoutExtension(sourceFilePath).StartsWith(QtCommon.MocFile.Prefix))
            {
                success = true;
                return null;
            }

            Opus.Core.Target target = objectFile.OwningNode.Target;

            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;
            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            bool isCPlusPlus = false;
            if (objectFile.Options is C.ICPlusPlusCompilerOptions)
            {
                isCPlusPlus = true;
            }

            NodeData nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("SOURCES", sourceFilePath);
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            if (isCPlusPlus)
            {
                nodeData.AddUniqueVariable("CXXFLAGS", commandLineBuilder);
                nodeData.AddUniqueVariable("QMAKE_CXX", new Opus.Core.StringArray(compilerInstance.ExecutableCPlusPlus(target)));
            }
            else
            {
                nodeData.AddUniqueVariable("CFLAGS", commandLineBuilder);
                Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;
                nodeData.AddUniqueVariable("QMAKE_CC", new Opus.Core.StringArray(compilerTool.Executable(target)));
            }
            nodeData.AddUniqueVariable("OBJECTS_DIR", new Opus.Core.StringArray(compilerOptions.OutputDirectoryPath));

            success = true;
            return nodeData;
        }
    }
}