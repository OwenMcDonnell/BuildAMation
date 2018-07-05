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
using System.Linq;
#endif
namespace XcodeProjectProcessor
{
    #if BAM_V2
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        System.Attribute
    {
        public EnumAttribute(
            object key,
            string value)
        {
            this.Key = key as System.Enum;
            this.Value = value;
        }

        public System.Enum Key
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }
    }

    public static class XcodeCompile
    {
        public static void
        Execute(
            Bam.Core.Module module)
        {
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings toolSettings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            var moduleType = typeof(Bam.Core.Module);
            var xcodeConfigurationType = typeof(XcodeBuilder.Configuration);
            foreach (var i in toolSettings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, moduleType, xcodeConfigurationType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2}, {3})",
                        conversionClass.ToString(),
                        i.ToString(),
                        moduleType,
                        xcodeConfigurationType);
                }
                try
                {
                    method.Invoke(null, new object[] { toolSettings, module, configuration });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "Xcode conversion error:");
                }
            }
        }
    }
}
