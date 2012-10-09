// <copyright file="BaseOptionCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class BaseOptionCollection : System.Collections.IEnumerable, System.ICloneable
    {
        protected System.Collections.Generic.Dictionary<string, Option> table = new System.Collections.Generic.Dictionary<string, Option>();

        public OutputPaths OutputPaths
        {
            get;
            private set;
        }

        public BaseOptionCollection()
        {
            this.OutputPaths = new OutputPaths();
        }

        public Option this[string key]
        {
            get
            {
                if (!this.Contains(key))
                {
                    string message = System.String.Format("Option '{0}' has not been registered in collection '{1}'. Is a default value missing?\n", key, this.ToString());
                    message += "Options registered are:\n";
                    foreach (string keyName in this.table.Keys)
                    {
                        message += System.String.Format("\t'{0}'\n", keyName);
                    }
                    throw new Exception(message, false);
                }

                return this.table[key];
            }

            set
            {
                this.table[key] = value;
            }
        }

        public virtual void SetNodeOwnership(DependencyNode node)
        {
            throw new Exception(System.String.Format("SetNodeOwnership needs to be overridden for '{0}' in module '{1}' of type '{2}' from package '{3}' for target '{4}'", this.ToString(), node.UniqueModuleName, node.Module.GetType().BaseType.ToString(), node.Package.ToString(), node.Target.ToString()), false);
        }

        protected virtual void SetDelegates(DependencyNode node)
        {
            throw new Exception(System.String.Format("SetDelegates needs to be overridden for '{0}' in module '{1}' of type '{2}' from package '{3}' for target '{4}'", this.ToString(), node.UniqueModuleName, node.Module.GetType().BaseType.ToString(), node.Package.ToString(), node.Target.ToString()), false);
        }

        public virtual object Clone()
        {
            System.Type optionsType = this.GetType();
            BaseOptionCollection clonedOptions = OptionCollectionFactory.CreateOptionCollection(optionsType);

            foreach (System.Collections.Generic.KeyValuePair<string, Option> option in this.table)
            {
                clonedOptions.table.Add(option.Key, option.Value.Clone() as Option);
            }
            return clonedOptions;
        }

        private void InvokeSetHandler(System.Reflection.MethodInfo setHandler, Option option)
        {
            if (null != setHandler)
            {
                if (2 != setHandler.GetParameters().Length)
                {
                    throw new Exception(System.String.Format("SetHandler requires the signature 'void {0}(BaseOptionCollection, Option)', not '{1}'", setHandler.Name, setHandler.ToString()));
                }

                setHandler.Invoke(null, new object[] { this, option });
            }
        }

        public void ProcessNamedSetHandler(string setHandlerName, Option option)
        {
            System.Type type = this.GetType();
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Static |           // don't need an instance
                                                          System.Reflection.BindingFlags.NonPublic |        // generally hidden - should be protected
                                                          System.Reflection.BindingFlags.FlattenHierarchy;  // bring in protected static functions
            InvokeSetHandler(type.GetMethod(setHandlerName, bindingFlags), option);
        }

#if false
        // this is private as I don't think it's needed anywhere now that we have the FinalizeOptions method
        // but it's kept around just in case
        private void ProcessAllSetHandlers()
        {
            System.Type type = this.GetType();
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Static |           // don't need an instance
                                                          System.Reflection.BindingFlags.NonPublic |        // generally hidden - should be protected
                                                          System.Reflection.BindingFlags.FlattenHierarchy;  // bring in protected static functions
            foreach (System.Collections.Generic.KeyValuePair<string, Option> option in this.table)
            {
                string setHandlerName = System.String.Format("{0}SetHandler", option.Key);
                InvokeSetHandler(type.GetMethod(setHandlerName, bindingFlags), option.Value);
            }
        }
#endif

        public virtual void FinalizeOptions(Opus.Core.Target target)
        {
            // do nothing
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public bool Contains(string key)
        {
            return this.table.ContainsKey(key);
        }

        protected Type GetValueTypeOption<Type>(string optionName) where Type : struct
        {
            return (this[optionName] as Core.ValueTypeOption<Type>).Value;
        }

        protected void SetValueTypeOption<Type>(string optionName, Type value) where Type : struct
        {
            if (this.Contains(optionName))
            {
                (this[optionName] as Core.ValueTypeOption<Type>).Value = value;
            }
            else
            {
                this[optionName] = new Core.ValueTypeOption<Type>(value);
            }
        }

        protected Type GetReferenceTypeOption<Type>(string optionName) where Type : class
        {
            return (this[optionName] as Core.ReferenceTypeOption<Type>).Value;
        }

        protected void SetReferenceTypeOption<Type>(string optionName, Type value) where Type : class
        {
            if (this.Contains(optionName))
            {
                (this[optionName] as Core.ReferenceTypeOption<Type>).Value = value;
            }
            else
            {
                this[optionName] = new Core.ReferenceTypeOption<Type>(value);
            }
        }

        public void FilterOutputPaths(System.Enum filter, StringArray paths)
        {
            System.Type filterType = filter.GetType();
            int filterValue = System.Convert.ToInt32(filter);

            Opus.Core.OutputPaths outputPaths = this.OutputPaths;
            if (State.RunningMono)
            {
                foreach (System.Enum key in outputPaths.Types)
                {
                    if (key.GetType() != filterType)
                    {
                        throw new Exception("Incompatible enum type comparison", false);
                    }

                    int keyValue = System.Convert.ToInt32(key);

                    if (keyValue == (filterValue & keyValue))
                    //if (o.Key.Includes(filter))
                    {
                        string path = outputPaths[key];
                        paths.Add(path);
                    }
                }
            }
            else
            {
                // TODO: this causes a System.InvalidCastException, Cannot cast from source type to destination type
                // because it's a SortedDictionary? Can't find any reference to this though
                foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> o in outputPaths)
                {
                    if (o.Key.GetType() != filterType)
                    {
                        throw new Exception("Incompatible enum type comparison", false);
                    }

                    int keyValue = System.Convert.ToInt32(o.Key);

                    if (keyValue == (filterValue & keyValue))
                    //if (o.Key.Includes(filter))
                    {
                        paths.Add(o.Value);
                    }
                }
            }
        }
    }
}