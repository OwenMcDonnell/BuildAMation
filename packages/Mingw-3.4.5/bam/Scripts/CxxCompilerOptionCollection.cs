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
namespace Mingw
{
    public sealed partial class CxxCompilerOptionCollection :
        MingwCommon.CxxCompilerOptionCollection
    {
        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // TODO: can this be moved to MingwCommon? (difference in root C folders)
            var target = node.Target;
            var mingwToolset = target.Toolset as MingwCommon.Toolset;

            var cCompilerOptions = this as C.ICCompilerOptions;

            // using [0] as we want the one in the root include folder
            var cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[0], "c++");
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath);

            var cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Version);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath2);

            // TODO: commenting these two lines out reveals an error on Mingw Test9-dev
            var cppIncludePath3 = System.IO.Path.Combine(cppIncludePath2, mingwToolset.MingwDetail.Target);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath3);
        }
    }
}
