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
namespace VisualStudioProcessor
{
#if BAM_V2
    public abstract class BaseAttribute :
        System.Attribute
    {
        protected BaseAttribute()
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        BaseAttribute
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

    public static class VSSolutionConversion
    {
        public static void
        Convert(
            Bam.Core.Settings settings,
            Bam.Core.Module module, // cannot use settings.Module as this may be null for per-file settings
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition = null)
        {
            var commandLine = new Bam.Core.StringArray();
            Bam.Core.Log.MessageAll("Module: {0}", module.ToString());
            Bam.Core.Log.MessageAll("Settings: {0}", settings.ToString());
            foreach (var settings_interface in settings.Interfaces())
            {
                Bam.Core.Log.MessageAll(settings_interface.ToString());
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    // must use the fully qualified property name from the originating interface
                    // to look for the instance in the concrete settings class
                    // this is to allow for the same property leafname to appear in multiple interfaces
                    var full_property_interface_name = System.String.Join(".", new[] { interface_property.DeclaringType.FullName, interface_property.Name });
                    var settings_property = settings.Properties.First(
                        item => full_property_interface_name == item.Name
                    );
                    Bam.Core.Log.MessageAll("\t{0}", settings_property.ToString());
                    var attributeArray = settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        Bam.Core.Log.MessageAll("\t\tNo attrs");
                        continue;
                    }
                }
            }
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            var moduleType = typeof(Bam.Core.Module);
            var vsSettingsGroupType = typeof(VSSolutionBuilder.VSSettingsGroup);
            var stringType = typeof(string);
            foreach (var i in settings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, moduleType, vsSettingsGroupType, stringType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2}, {3})",
                        conversionClass.ToString(),
                        i.ToString(),
                        moduleType,
                        vsSettingsGroupType,
                        stringType);
                }
                try
                {
                    method.Invoke(null, new object[] { settings, module, vsSettingsGroup, condition });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "VisualStudio conversion error:");
                }
            }
        }
    }
}
