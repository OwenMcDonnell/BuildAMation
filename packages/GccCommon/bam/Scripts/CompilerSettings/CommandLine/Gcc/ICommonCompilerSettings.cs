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
namespace GccCommon
{
    public static partial class CommandLineImplementation
    {
        public static void
        Convert(
            this GccCommon.ICommonCompilerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            if (settings.PositionIndependentCode.HasValue)
            {
                if (settings.PositionIndependentCode.Value)
                {
                    commandLine.Add("-fPIC");
                }
            }
            if (settings.AllWarnings.HasValue)
            {
                if (settings.AllWarnings.Value)
                {
                    commandLine.Add("-Wall");
                }
                else
                {
                    commandLine.Add("-Wno-all");
                }
            }
            if (settings.ExtraWarnings.HasValue)
            {
                if (settings.ExtraWarnings.Value)
                {
                    commandLine.Add("-Wextra");
                }
                else
                {
                    commandLine.Add("-Wno-extra");
                }
            }
            if (settings.Pedantic.HasValue)
            {
                if (settings.Pedantic.Value)
                {
                    commandLine.Add("-Wpedantic");
                }
                else
                {
                    commandLine.Add("-Wno-pedantic");
                }
            }
            if (settings.Visibility.HasValue)
            {
                switch (settings.Visibility.Value)
                {
                case EVisibility.Default:
                    commandLine.Add("-fvisibility=default");
                    break;
                case EVisibility.Hidden:
                    commandLine.Add("-fvisibility=hidden");
                    break;
                case EVisibility.Internal:
                    commandLine.Add("-fvisibility=internal");
                    break;
                case EVisibility.Protected:
                    commandLine.Add("-fvisibility=protected");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized visibility, {0}", settings.Visibility.Value.ToString());
                }
            }
            if (settings.StrictAliasing.HasValue)
            {
                if (settings.StrictAliasing.Value)
                {
                    commandLine.Add("-fstrict-aliasing");
                }
                else
                {
                    commandLine.Add("-fno-strict-aliasing");
                }
            }
        }
    }
}
