#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace Publisher
{
    public abstract class DebugSymbolCollation :
        Bam.Core.Module
    {
        protected DebugSymbolCollation()
        {
        }

        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
        }

        public void
        CreateSymbolsFrom<DependentModule>() where DependentModule : Collation, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            this.DependsOn(dependent);

            foreach (CollatedObject req in dependent.Requirements)
            {
                var source = req.SourceModule;
                if (!(req is CollatedFile))
                {
                    continue;
                }
                if (!(source is C.ConsoleApplication))
                {
                    continue;
                }
                if (Bam.Core.OSUtilities.IsWindowsHosting)
                {
                    // TODO:
                }
                else if (Bam.Core.OSUtilities.IsLinuxHosting)
                {
                    var createDebugSymbols = Bam.Core.Module.Create<ObjCopyModule>(preInitCallback: module =>
                        {
                            module.Macros.Add("DebugSymbolRoot", module.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)-$(config)"));
                        });
                    this.DependsOn(createDebugSymbols);
                    createDebugSymbols.SourceModule = req;
                    createDebugSymbols.PrivatePatch(settings =>
                        {
                            var objCopySettings = settings as IObjCopyToolSettings;
                            objCopySettings.Mode = EObjCopyToolMode.OnlyKeepDebug;
                        });

                    var linkDebugSymbols = Bam.Core.Module.Create<ObjCopyModule>(preInitCallback: module =>
                        {
                            module.Macros.Add("DebugSymbolRoot", module.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)-$(config)"));
                        });
                    this.DependsOn(linkDebugSymbols);
                    linkDebugSymbols.Requires(createDebugSymbols);
                    linkDebugSymbols.SourceModule = req;
                    linkDebugSymbols.PrivatePatch(settings =>
                        {
                            var objCopySettings = settings as IObjCopyToolSettings;
                            objCopySettings.Mode = EObjCopyToolMode.AddGNUDebugLink;
                        });
                }
                else if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    var createDebugSymbols = Bam.Core.Module.Create<DSymUtilModule>(preInitCallback: module =>
                        {
                            module.Macros.Add("DebugSymbolRoot", module.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)-$(config)"));
                        });
                    this.DependsOn(createDebugSymbols);
                    createDebugSymbols.SourceModule = req;
                }
            }
        }
    }
}