// <copyright file="Utilities.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public static class Utilities
    {
        public static void GetBesideModule(object module,
                                           Opus.Core.Target target,
                                           out BesideModuleAttribute attribute,
                                           out System.Type dependentModule)
        {
            attribute = null;
            dependentModule = null;

            System.Reflection.BindingFlags bindingFlags =
                System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = module.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(BesideModuleAttribute), false);
                if (1 == attributes.Length)
                {
                    if (null != attribute)
                    {
                        throw new Opus.Core.Exception("Cannot set more than one BesideModule");
                    }

                    attribute = attributes[0] as BesideModuleAttribute;
                    var value = field.GetValue(module);
                    if (value is System.Type)
                    {
                        dependentModule = field.GetValue(module) as System.Type;
                    }
                    else
                    {
                        throw new Opus.Core.Exception("Expected BesideModule field '{0}' to be of type System.Type", field.Name);
                    }
                }
            }

            if (null != attribute)
            {
                if (!Opus.Core.TargetUtilities.MatchFilters(target, attribute))
                {
                    dependentModule = null;
                    attribute = null;
                }
            }
        }
    }
}
