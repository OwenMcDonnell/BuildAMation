﻿// <copyright file="BaseModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// BaseModules are the base class for all real modules in package scripts.
    /// These are constructed by the Opus Core when they are required.
    /// Nested modules that appear as fields are either constructed automatically by
    /// the default constructor of their parent, or in the custom construct required to be
    /// written by the package author. As such, there must always be a default constructor
    /// in BaseModule.
    /// </summary>
    public abstract class BaseModule :
        IModule
    {
        private readonly LocationKey PackageDirKey = new LocationKey("PackageDirectory", ScaffoldLocation.ETypeHint.Directory);

        private void
        StubOutputLocations(
            System.Type moduleType)
        {
            this.Locations[State.ModuleBuildDirLocationKey] = new ScaffoldLocation(ScaffoldLocation.ETypeHint.Directory);

            var toolAssignment = moduleType.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
            // this is duplicating work, as the toolset is in the Target.Toolset, but passing a Target down to
            // the BaseModule constructor will break a lot of existing scripts, and their simplicity
            // TODO: it may be considered a change in a future version
            var toolset = ModuleUtilities.GetToolsetForModule(moduleType);
            var toolAttr = toolAssignment[0] as ModuleToolAssignmentAttribute;
            if (!toolset.HasTool(toolAttr.ToolType))
            {
                return;
            }
            var tool = toolset.Tool(toolAttr.ToolType);
            if (null != tool)
            {
                foreach (var locationKey in tool.OutputLocationKeys(this))
                {
                    this.Locations[locationKey] = new ScaffoldLocation(locationKey.Type);
                }
            }
        }

        protected
        BaseModule()
        {
            this.ProxyPath = new ProxyModulePath();
            this.Locations = new LocationMap();
            this.Locations[State.BuildRootLocationKey] = State.BuildRootLocation;

            var moduleType = this.GetType();
            this.StubOutputLocations(moduleType);

            var packageName = moduleType.Namespace;
            var package = State.PackageInfo[packageName];
            if (null != package)
            {
                var root = new ScaffoldLocation(package.Identifier.Location, this.ProxyPath, ScaffoldLocation.ETypeHint.Directory, Location.EExists.Exists);
                this.PackageLocation = root;
            }
        }

        /// <summary>
        /// Locations are only valid for named modules
        /// </summary>
        public LocationMap Locations
        {
            get;
            private set;
        }

        public Location PackageLocation
        {
            get
            {
                return this.Locations[PackageDirKey];
            }

            private set
            {
                this.Locations[PackageDirKey] = value;
            }
        }

        public event UpdateOptionCollectionDelegate UpdateOptions;

        public virtual BaseOptionCollection Options
        {
            get;
            set;
        }

        public ProxyModulePath ProxyPath
        {
            get;
            private set;
        }

        public void
        ExecuteOptionUpdate(
            Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this as IModule, target);
            }
        }

        private DependencyNode owningNode = null;
        public DependencyNode OwningNode
        {
            get
            {
                return this.owningNode;
            }

            set
            {
                if (null != this.owningNode)
                {
                    throw new Exception("Module {0} cannot have it's node reassigned to {1}", this.owningNode.UniqueModuleName, value.UniqueModuleName);
                }

                this.owningNode = value;
            }
        }
    }
}
