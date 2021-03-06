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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Abstract base class for all settings
    /// </summary>
    public abstract class Settings
    {
        private class InterfaceData
        {
            public System.Type InterfaceType;
            public System.Reflection.MethodInfo emptyMethod;
            public System.Reflection.MethodInfo defaultMethod;
        }

        private class SettingsInterfaces
        {
            public Bam.Core.Array<InterfaceData> Data = new Array<InterfaceData>();
        }

        // TODO: Could this be System.Collections.Concurrent.ConcurrentDictionary to avoid the explicit lock below?
        private static System.Collections.Generic.Dictionary<System.Type, SettingsInterfaces> Cache = new System.Collections.Generic.Dictionary<System.Type, SettingsInterfaces>();

        private static SettingsInterfaces
        GetSettingsInterfaces(
            System.Type settingsType,
            System.Collections.Generic.IEnumerable<System.Type> currentInterfaces)
        {
            lock (Cache)
            {
                if (!Cache.ContainsKey(settingsType))
                {
                    var attributeType = typeof(SettingsExtensionsAttribute);
                    var interfaces = new SettingsInterfaces();
                    foreach (var i in currentInterfaces)
                    {
                        var attributeArray = i.GetCustomAttributes(attributeType, false);
                        if (0 == attributeArray.Length)
                        {
                            throw new Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                        }

                        var newData = new InterfaceData();
                        newData.InterfaceType = i;

                        var attribute = attributeArray[0] as SettingsExtensionsAttribute;
                        // TODO: the Empty function could be replaced by the auto-property initializers in C#6.0 (when Mono catches up)
                        // although it won't then be a centralized definition, so the extension method as-is is probably better
                        newData.emptyMethod = attribute.GetMethod("Empty", new[] { i });
                        newData.defaultMethod = attribute.GetMethod("Defaults", new[] { i, typeof(Module) });
                        interfaces.Data.Add(newData);
                    }
                    Cache.Add(settingsType, interfaces);
                }

                return Cache[settingsType];
            }
        }

        /// <summary>
        /// Return each interface implemented, implementing ISettingsBase, and ordered by precedence with
        /// SettingsPrecedenceAttribute.
        /// </summary>
        public System.Collections.Generic.IEnumerable<System.Type>
        Interfaces()
        {
            // find true interfaces
            var baseI = typeof(ISettingsBase);
            // note that GetInterfaces does not return in a deterministic order
            // so use any precedence to order those that need to be (highest first)
            var interfaces = this.GetType().GetInterfaces().
                Where(item => (item != baseI) && baseI.IsAssignableFrom(item)).
                OrderByDescending(
                    item =>
                    {
                        var precedenceAttribs = item.GetCustomAttributes(typeof(SettingsPrecedenceAttribute), false);
                        if (precedenceAttribs.Length > 0)
                        {
                            return (precedenceAttribs[0] as SettingsPrecedenceAttribute).Order;
                        }
                        return 0;
                    });

            foreach (var i in interfaces)
            {
                yield return i;
            }
        }

        /// <summary>
        /// For all settings interfaces, optionally calling Empty method and Default method
        /// in the extensions class defined by SettingsExtensionsAttribute on each interface.
        /// </summary>
        /// <param name="module">Module.</param>
        /// <param name="emptyFirst">If set to <c>true</c> empty first.</param>
        /// <param name="useDefaults">If set to <c>true</c> use defaults.</param>
        protected void
        InitializeAllInterfaces(
            Module module,
            bool emptyFirst,
            bool useDefaults)
        {
            this.Module = module;
            var data = GetSettingsInterfaces(this.GetType(), this.Interfaces());
            foreach (var i in data.Data)
            {
                if (emptyFirst)
                {
                    if (null != i.emptyMethod)
                    {
                        i.emptyMethod.Invoke(null, new[] { this });
                    }
                }
                if (useDefaults)
                {
                    if (null != i.defaultMethod)
                    {
                        i.defaultMethod.Invoke(null, new object[] { this, module });
                    }
                }
            }
            if (useDefaults && (null != LocalPolicy))
            {
                LocalPolicy.DefineLocalSettings(this, module);
            }
        }

        /// <summary>
        /// Get or set the Module associated with the Settings instance.
        /// </summary>
        /// <value>The module.</value>
        public Module Module
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines the local policy to use for all settings
        /// </summary>
        /// <value>The local policy.</value>
        static public ISitePolicy LocalPolicy
        {
            get;
            set;
        }
    }
}
