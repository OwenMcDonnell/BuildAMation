// <copyright file="ToolchainAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.ToolchainAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class ToolchainAction : Opus.Core.IActionWithArguments
    {
        private string Toolchain
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Toolchain = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.toolchain";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the toolchain used for building C code";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("C toolchain is '{0}'", this.Toolchain);

            if (!Opus.Core.State.HasCategory("Toolchains"))
            {
                Opus.Core.State.AddCategory("Toolchains");
            }

            if (Opus.Core.State.Has("Toolchains", "C"))
            {
                throw new Opus.Core.Exception(System.String.Format("Toolchain for 'C' has already been defined as '{0}'", Opus.Core.State.Get("Toolchains", "C") as string));
            }

            Opus.Core.State.Add<string>("Toolchains", "C", this.Toolchain);
            Opus.Core.State.Add<string>("Toolchains", "C.CPlusPlus", this.Toolchain);

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
            map[typeof(ICompilerTool)]    = this.Toolchain;
            map[typeof(ICxxCompilerTool)] = this.Toolchain;
            map[typeof(ILinkerTool)]      = this.Toolchain;
            map[typeof(IArchiverTool)]    = this.Toolchain;
            map[typeof(Win32ResourceCompilerBase)] = this.Toolchain;
#if false
            map[typeof(C.Toolchain)]   = this.Toolchain;
#endif

            return true;
        }
    }
}
