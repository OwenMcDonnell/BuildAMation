#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace CodeGenTest
{
#if BAM_V2
    public static partial class MakeFileSupport
    {
        public static void
        GenerateSource(
            GeneratedSourceModule module)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(module);
            var rule = meta.AddRule();
            rule.AddTarget(module.GeneratedPaths[GeneratedSourceModule.SourceFileKey]);
            foreach (var input in module.InputModules)
            {
                System.Diagnostics.Debug.Assert(input.Key == C.ObjectFile.ObjectFileKey);
                rule.AddPrerequisite(input.Value, input.Key);
            }

            var tool_metadata = ((module.Tool as Bam.Core.Module).MetaData as MakeFileBuilder.MakeFileMeta);
            rule.AddPrerequisite(tool_metadata.Rules[0].FirstTarget);

            var tool = module.Tool as Bam.Core.ICommandLineTool;
            if (null != tool.EnvironmentVariables)
            {
                meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
            }

            var command = new System.Text.StringBuilder();
            command.AppendFormat("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(command.ToString());

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }
        }
    }
#else
    public sealed class MakeFileGenerateSource :
        IGeneratedSourcePolicy
    {
        void
        IGeneratedSourcePolicy.GenerateSource(
            GeneratedSourceModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString generatedFilePath)
        {
            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(generatedFilePath);

            var buildTool = (compiler as Bam.Core.Module).MetaData as MakeFileBuilder.MakeFileMeta;
            rule.AddOrderOnlyDependency(System.String.Format("$({0})", buildTool.Rules[0].FirstTarget.VariableName));

            // TODO: change this to a configuration directory really
            var output_dir = Bam.Core.IOWrapper.EncloseSpaceContainingPathWithDoubleQuotes(Bam.Core.Graph.Instance.BuildRoot);

            var commandLineArgs = new Bam.Core.StringArray();
            commandLineArgs.Add(output_dir);
            commandLineArgs.Add("Generated");

            var command = new System.Text.StringBuilder();
            if (MakeFileBuilder.MakeFileCommonMetaData.IsNMAKE)
            {
                command.AppendFormat("{0} {1} $**", compiler.Executable.ToStringQuoteIfNecessary(), commandLineArgs.ToString(' '));
            }
            else
            {
                command.AppendFormat("{0} {1} $^", compiler.Executable.ToStringQuoteIfNecessary(), commandLineArgs.ToString(' '));
            }
            rule.AddShellCommand(command.ToString());
        }
    }
#endif
}
