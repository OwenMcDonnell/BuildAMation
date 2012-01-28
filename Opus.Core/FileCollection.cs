// <copyright file="FileCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class FileCollection : System.ICloneable, System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<string> filePaths = new System.Collections.Generic.List<string>();

        public FileCollection()
        {
        }

        public FileCollection(params FileCollection[] collections)
        {
            foreach (FileCollection collection in collections)
            {
                foreach (string path in collection)
                {
                    this.Add(path);
                }
            }
        }

        public object Clone()
        {
            FileCollection clone = new FileCollection();
            clone.filePaths.AddRange(this.filePaths);
            return clone;
        }

        public void Add(string absolutePath)
        {
            this.filePaths.Add(absolutePath);
        }

        public void AddRange(StringArray absolutePathArray)
        {
            foreach (string path in absolutePathArray)
            {
                this.filePaths.Add(path);
            }
        }

        public string this[int index]
        {
            get
            {
                return this.filePaths[index];
            }
        }

        public int Count
        {
            get
            {
                return this.filePaths.Count;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.filePaths.GetEnumerator();
        }

        public void AddRelativePaths(object owner, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(owner);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", owner.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            ProxyModulePath proxyPath = (owner as IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            StringArray paths = File.GetFiles(packagePath, pathSegments);
            foreach (string path in paths)
            {
                this.filePaths.Add(path);
            }
        }

        public void Include(object module, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", module.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            ProxyModulePath proxyPath = (module as IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            StringArray paths = File.GetFiles(packagePath, pathSegments);
            foreach (string path in paths)
            {
                this.filePaths.Add(path);
            }
        }

        public void Exclude(object module, params string[] pathSegments)
        {
            PackageInformation package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception(System.String.Format("Unable to locate package '{0}'", module.GetType().Namespace), false);
            }

            string packagePath = package.Identifier.Path;
            ProxyModulePath proxyPath = (module as IModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            StringArray paths = File.GetFiles(packagePath, pathSegments);
            foreach (string path in paths)
            {
                this.filePaths.Remove(path);
            }
        }

        public void AddRelativePaths(string baseDirectory, string relativePath)
        {
            if (!System.IO.Directory.Exists(baseDirectory))
            {
                throw new Exception(System.String.Format("Base directory '{0}' does not exist", baseDirectory), false);
            }

            StringArray paths = File.GetFiles(baseDirectory, relativePath);
            foreach (string path in paths)
            {
                this.filePaths.Add(path);
            }
        }
    }
}