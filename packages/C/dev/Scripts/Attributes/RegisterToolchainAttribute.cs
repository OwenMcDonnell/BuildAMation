// <copyright file="RegisterToolchainAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public RegisterToolchainAttribute(string name,
                                          System.Type cCompilerType,
                                          System.Type cCompilerOptionType,
                                          System.Type cppCompilerType,
                                          System.Type cppCompilerOptionType,
                                          System.Type linkerType,
                                          System.Type linkerOptionType,
                                          System.Type archiverType,
                                          System.Type archiverOptionType,
                                          System.Type win32ResourceCompilerType,
                                          System.Type win32ResourceCompilerOptionType)
        {
            if (!typeof(C.Compiler).IsAssignableFrom(cCompilerType))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler type '{0}' does not implement the base class {1}", cCompilerType.ToString(), typeof(C.Compiler).ToString()), false);
            }
            if (!typeof(C.ICCompilerOptions).IsAssignableFrom(cCompilerOptionType))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", cCompilerOptionType.ToString(), typeof(C.ICCompilerOptions).ToString()), false);
            }

            if (!typeof(C.CxxCompiler).IsAssignableFrom(cppCompilerType))
            {
                throw new Opus.Core.Exception(System.String.Format("C++ Compiler type '{0}' does not implement the base class {1}", cppCompilerType.ToString(), typeof(C.CxxCompiler).ToString()), false);
            }
            if (!typeof(C.ICPlusPlusCompilerOptions).IsAssignableFrom(cppCompilerOptionType))
            {
                throw new Opus.Core.Exception(System.String.Format("C++ Compiler option type '{0}' does not implement the interface {1}", cppCompilerOptionType.ToString(), typeof(C.ICPlusPlusCompilerOptions).ToString()), false);
            }

            if (!typeof(C.Linker).IsAssignableFrom(linkerType))
            {
                throw new Opus.Core.Exception(System.String.Format("Linker type '{0}' does not implement the base class {1}", linkerType.ToString(), typeof(C.Linker).ToString()), false);
            }
            if (!typeof(C.ILinkerOptions).IsAssignableFrom(linkerOptionType))
            {
                throw new Opus.Core.Exception(System.String.Format("Linker option type '{0}' does not implement the interface {1}", linkerOptionType.ToString(), typeof(C.ILinkerOptions).ToString()), false);
            }

            if (!typeof(C.Archiver).IsAssignableFrom(archiverType))
            {
                throw new Opus.Core.Exception(System.String.Format("Archiver type '{0}' does not implement the base class {1}", archiverType.ToString(), typeof(C.Archiver).ToString()), false);
            }
            if (!typeof(C.IArchiverOptions).IsAssignableFrom(archiverOptionType))
            {
                throw new Opus.Core.Exception(System.String.Format("Archiver option type '{0}' does not implement the interface {1}", archiverOptionType.ToString(), typeof(C.IArchiverOptions).ToString()), false);
            }

            if (null != win32ResourceCompilerType)
            {
                if (!typeof(C.Win32ResourceCompilerBase).IsAssignableFrom(win32ResourceCompilerType))
                {
                    throw new Opus.Core.Exception(System.String.Format("Win32 resource compiler type '{0}' does not implement the base class {1}", win32ResourceCompilerType.ToString(), typeof(C.Win32ResourceCompilerBase).ToString()), false);
                }
                // TODO: there is no interface for the options? WTF?
            }

            System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
            map[typeof(C.Compiler)] = new ToolAndOptions(cCompilerType, cCompilerOptionType);
            map[typeof(C.CxxCompiler)] = new ToolAndOptions(cppCompilerType, cppCompilerOptionType);
            map[typeof(C.Linker)] = new ToolAndOptions(linkerType, linkerOptionType);
            map[typeof(C.Archiver)] = new ToolAndOptions(archiverType, archiverOptionType);
            map[typeof(C.Win32ResourceCompilerBase)] = new ToolAndOptions(win32ResourceCompilerType, win32ResourceCompilerOptionType);

            if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
            {
                Opus.Core.State.AddCategory("ToolchainTypeMap");
            }
            Opus.Core.State.Add("ToolchainTypeMap", name, map);
        }
    }
}
