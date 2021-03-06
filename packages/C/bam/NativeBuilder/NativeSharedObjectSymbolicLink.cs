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
using Bam.Core;
namespace C
{
    public sealed class NativeSharedObjectSymbolicLink :
        ISharedObjectSymbolicLinkPolicy
    {
        void
        ISharedObjectSymbolicLinkPolicy.Symlink(
            SharedObjectSymbolicLink sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.PreBuiltTool tool,
            ConsoleApplication target)
        {
            var commandLine = new Bam.Core.StringArray();
            commandLine.Add("-s");
            commandLine.Add("-f");
            var sourceFile = sender.CreateTokenizedString("@filename($(0))", target.GeneratedPaths[ConsoleApplication.Key]);
            lock (sourceFile)
            {
                if (!sourceFile.IsParsed)
                {
                    sourceFile.Parse();
                }
            }
            commandLine.Add(sourceFile.ToStringQuoteIfNecessary());
            var destination = sender.CreateTokenizedString("@dir($(0))/$(1)", target.GeneratedPaths[ConsoleApplication.Key], target.Macros[sender.Macros["SymlinkUsage"].ToString()]);
            lock (destination)
            {
                if (!destination.IsParsed)
                {
                    destination.Parse();
                }
            }
            commandLine.Add(destination.ToStringQuoteIfNecessary());
            CommandLineProcessor.Processor.Execute(context, tool, commandLine);
        }
    }
}
