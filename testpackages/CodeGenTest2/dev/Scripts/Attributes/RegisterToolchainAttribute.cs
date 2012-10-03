namespace CodeGenTest2
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolchainAttribute : Opus.Core.RegisterToolchainAttribute
    {
        public RegisterToolchainAttribute(string name)
        {
            if (!typeof(ICodeGenOptions).IsAssignableFrom(typeof(CodeGenOptions)))
            {
                throw new Opus.Core.Exception(System.String.Format("C Compiler option type '{0}' does not implement the interface {1}", typeof(CodeGenOptions).ToString(), typeof(ICodeGenOptions).ToString()), false);
            }

            {
                System.Collections.Generic.Dictionary<System.Type, ToolAndOptions> map = new System.Collections.Generic.Dictionary<System.Type, ToolAndOptions>();
                // TODO: there is a 1-1 mapping from tool to tool? Is that ok? We could simplify that to just the optionset type
                map[typeof(CodeGenTool)] = new ToolAndOptions(typeof(CodeGenTool), typeof(CodeGenOptions));

                if (!Opus.Core.State.HasCategory("ToolchainTypeMap"))
                {
                    Opus.Core.State.AddCategory("ToolchainTypeMap");
                }
                Opus.Core.State.Add("ToolchainTypeMap", name, map);
            }

            // TODO: we do this here because we know that this will be executed
            {
                // NEW STYLE: mapping each type of tool to it's toolchain (this is the default)
                System.Collections.Generic.Dictionary<System.Type, string> map = null;
                if (Opus.Core.State.Has("Toolchains", "Map"))
                {
                    map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
                }
                else
                {
                    map = new System.Collections.Generic.Dictionary<System.Type, string>();
                    Opus.Core.State.Add("Toolchains", "Map", map);
                }
                map[typeof(CodeGenTool)] = "CodeGenTest2";
            }
        }
    }
}
