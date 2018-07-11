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
#if BAM_V2
#else
namespace VisualCCommon
{
    public static partial class VSSolutionImplementation
    {
        public static void
        Convert(
            this VisualCCommon.ICommonCompilerSettings settings,
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            if (settings.NoLogo.GetValueOrDefault(false))
            {
                vsSettingsGroup.AddSetting("SuppressStartupBanner", settings.NoLogo.Value, condition);
            }

            if (settings.RuntimeLibrary.HasValue)
            {
                vsSettingsGroup.AddSetting("RuntimeLibrary", settings.RuntimeLibrary.Value.ToString(), condition);
            }

            if (settings.WarningLevel.HasValue)
            {
                if (EWarningLevel.Level0 == settings.WarningLevel.Value)
                {
                    vsSettingsGroup.AddSetting("WarningLevel", "TurnOffAllWarnings", condition);
                }
                else
                {
                    vsSettingsGroup.AddSetting("WarningLevel", System.String.Format("Level{0}", settings.WarningLevel.Value.ToString("D")), condition);
                }
            }

            if (settings.EnableLanguageExtensions.HasValue)
            {
                vsSettingsGroup.AddSetting("DisableLanguageExtensions", !settings.EnableLanguageExtensions.Value, condition);
            }

            if (settings.Optimization.HasValue)
            {
                var common_optimization = (settings as C.ICommonCompilerSettings).Optimization;
                if (common_optimization.HasValue && common_optimization.Value != C.EOptimization.Custom)
                {
                    throw new Bam.Core.Exception("Compiler specific optimizations can only be set when the common optimization is C.EOptimization.Custom");
                }

                System.Func<string> optimization = () =>
                {
                    switch (settings.Optimization.Value)
                    {
                        case EOptimization.Full:
                            return "Full";

                        default:
                            throw new Bam.Core.Exception("Unknown compiler optimization type, {0}", settings.Optimization.Value.ToString());
                    }
                };
                vsSettingsGroup.AddSetting("Optimization", optimization(), condition);
            }

            if (settings.IncreaseObjectFileSectionCount.HasValue)
            {
                if (settings.IncreaseObjectFileSectionCount.Value)
                {
                    vsSettingsGroup.AddSetting("AdditionalOptions", "-bigobj", condition);
                }
            }
        }
    }
}
#endif
