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
namespace MingwCommon
{
    public abstract class CommonCompilerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
#endif
        C.ICommonHasSourcePath,
        C.ICommonHasOutputPath,
        C.ICommonCompilerSettingsWin,
        C.ICommonCompilerSettings,
        C.IAdditionalSettings,
        ICommonCompilerSettings
    {
        protected CommonCompilerSettings(
            Bam.Core.Module module)
            :
            this(module, useDefaults: true)
        {}

        protected CommonCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
        {
            this.InitializeAllInterfaces(module, true, useDefaults);
        }

#if BAM_V2
#else
        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineImplementation), this, commandLine);
        }
#endif

#if BAM_V2
        [CommandLineProcessor.Enum(C.ECharacterSet.NotSet, "")]
        [CommandLineProcessor.Enum(C.ECharacterSet.Unicode, "-D_UNICODE")]
        [CommandLineProcessor.Enum(C.ECharacterSet.MultiByte, "-D_MBCS")]
#endif
        C.ECharacterSet? C.ICommonCompilerSettingsWin.CharacterSet
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("")]
#endif
        Bam.Core.TokenizedString C.ICommonHasSourcePath.SourcePath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-o ")]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-m32")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-m64")]
#endif
        C.EBit? C.ICommonCompilerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PreprocessorDefines("-D")]
#endif
        C.PreprocessorDefinitions C.ICommonCompilerSettings.PreprocessorDefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.IncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.SystemIncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ECompilerOutput.CompileOnly, "-c")]
        [CommandLineProcessor.Enum(C.ECompilerOutput.Preprocess, "-E")]
#endif
        C.ECompilerOutput? C.ICommonCompilerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-g", "")]
#endif
        bool? C.ICommonCompilerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
#endif
        bool? C.ICommonCompilerSettings.WarningsAsErrors
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EOptimization.Off, "-O0")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-O1")]
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")] // use Mingw specific settings
#endif
        C.EOptimization? C.ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ETargetLanguage.Default, "")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-x c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-x c++")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveC, "-x objective-c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveCxx, "-x objective-c++")]
#endif
        C.ETargetLanguage? C.ICommonCompilerSettings.TargetLanguage
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-fomit-frame-pointer", "-fno-omit-frame-pointer")]
#endif
        bool? C.ICommonCompilerSettings.OmitFramePointer
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-Wno-")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-U")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.PreprocessorUndefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-include ")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wall", "-Wno-all")]
#endif
        bool? ICommonCompilerSettings.AllWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wextra", "-Wno-extra")]
#endif
        bool? ICommonCompilerSettings.ExtraWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wpedantic", "-Wno-pedantic")]
#endif
        bool? ICommonCompilerSettings.Pedantic
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EVisibility.Default, "-fvisibility=default")]
        [CommandLineProcessor.Enum(EVisibility.Hidden, "-fvisibility=hidden")]
        [CommandLineProcessor.Enum(EVisibility.Internal, "-fvisibility=internal")]
        [CommandLineProcessor.Enum(EVisibility.Protected, "-fvisibility=protected")]
#endif
        EVisibility? ICommonCompilerSettings.Visibility
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-fstrict-aliasing", "-fno-strict-aliasing")]
#endif
        bool? ICommonCompilerSettings.StrictAliasing
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EOptimization.O3, "-O3")]
        [CommandLineProcessor.Enum(EOptimization.Ofast, "-Ofast")]
#endif
        EOptimization? ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }
    }
}