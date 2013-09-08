// <copyright file="PBXContainerItemProxy.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXContainerItemProxy : XCodeNodeData, IWriteableNode
    {
        public PBXContainerItemProxy(string name, XCodeNodeData remote, XCodeNodeData portal)
            : base(name)
        {
            this.Remote = remote;
            this.Portal = portal;
        }

        private XCodeNodeData Remote
        {
            get;
            set;
        }

        private XCodeNodeData Portal
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (this.Remote == null)
            {
                throw new Opus.Core.Exception("Remote was not set on this container proxy");
            }
            if (this.Portal == null)
            {
                throw new Opus.Core.Exception("Portal was not set on this container proxy");
            }

            writer.WriteLine("\t\t{0} /* PBXContainerItemProxy */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXContainerItemProxy;");
            writer.WriteLine("\t\t\tcontainerPortal = {0} /* {1} object */;", this.Portal.UUID, this.Portal.GetType().Name);
            writer.WriteLine("\t\t\tproxyType = 1;");
            writer.WriteLine("\t\t\tremoteGlobalIDString = {0};", this.Remote.UUID);
            writer.WriteLine("\t\t\tremoteInfo = {0};", this.Remote.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
