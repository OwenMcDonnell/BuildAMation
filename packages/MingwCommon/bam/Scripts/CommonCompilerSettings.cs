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
        CommandLineProcessor.IConvertToCommandLine,
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

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineImplementation), this, commandLine);
        }

        C.ECharacterSet? C.ICommonCompilerSettingsWin.CharacterSet
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("")]
#endif
        Bam.Core.TokenizedString C.ICommonCompilerSettings.SourcePath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-o ")]
#endif
        Bam.Core.TokenizedString C.ICommonCompilerSettings.OutputPath
        {
            get;
            set;
        }

        C.EBit? C.ICommonCompilerSettings.Bits
        {
            get;
            set;
        }

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

        bool? C.ICommonCompilerSettings.DebugSymbols
        {
            get;
            set;
        }

        bool? C.ICommonCompilerSettings.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.ICommonCompilerSettings.TargetLanguage
        {
            get;
            set;
        }

        bool? C.ICommonCompilerSettings.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerSettings.PreprocessorUndefines
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders
        {
            get;
            set;
        }

        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

        bool? ICommonCompilerSettings.AllWarnings
        {
            get;
            set;
        }

        bool? ICommonCompilerSettings.ExtraWarnings
        {
            get;
            set;
        }

        bool? ICommonCompilerSettings.Pedantic
        {
            get;
            set;
        }

        EVisibility? ICommonCompilerSettings.Visibility
        {
            get;
            set;
        }

        bool? ICommonCompilerSettings.StrictAliasing
        {
            get;
            set;
        }

        EOptimization? ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }
    }
}
