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
namespace ClangCommon
{
    public static partial class XcodeCompilerImplementation
    {
        public static void
        Convert(
            this ClangCommon.ICommonCompilerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
#if BAM_V2
#else
            if (settings.AllWarnings.HasValue)
            {
                var warnings = new XcodeBuilder.MultiConfigurationValue();
                if (settings.AllWarnings.Value)
                {
                    warnings.Add("-Wall");
                }
                else
                {
                    warnings.Add("-Wno-all");
                }
                configuration["WARNING_CFLAGS"] = warnings;
            }
            if (settings.ExtraWarnings.HasValue)
            {
                var warnings = new XcodeBuilder.MultiConfigurationValue();
                if (settings.ExtraWarnings.Value)
                {
                    warnings.Add("-Wextra");
                }
                else
                {
                    warnings.Add("-Wno-extra");
                }
                configuration["WARNING_CFLAGS"] = warnings;
            }
            if (settings.Pedantic.HasValue)
            {
                configuration["GCC_WARN_PEDANTIC"] = new XcodeBuilder.UniqueConfigurationValue(settings.Pedantic.Value ? "YES" : "NO");
            }
#endif
            if (settings.Visibility.HasValue)
            {
                configuration["GCC_SYMBOLS_PRIVATE_EXTERN"] = new XcodeBuilder.UniqueConfigurationValue((settings.Visibility.Value == EVisibility.Default) ? "NO" : "YES");
            }
            if (settings.StrictAliasing.HasValue)
            {
                configuration["GCC_STRICT_ALIASING"] = new XcodeBuilder.UniqueConfigurationValue(settings.StrictAliasing.Value ? "YES" : "NO");
            }
            if (settings.Optimization.HasValue)
            {
                var common_optimization = (settings as C.ICommonCompilerSettings).Optimization;
                if (common_optimization.HasValue && common_optimization.Value != C.EOptimization.Custom)
                {
                    throw new Bam.Core.Exception("Compiler specific optimizations can only be set when the common optimization is C.EOptimization.Custom");
                }

                switch (settings.Optimization.Value)
                {
                    case EOptimization.O1:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("1");
                        break;
                    case EOptimization.O3:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("3");
                        break;
                    case EOptimization.Ofast:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("fast");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported Clang specific optimization, {0}", settings.Optimization.Value);
                }
            }
        }
    }
}
