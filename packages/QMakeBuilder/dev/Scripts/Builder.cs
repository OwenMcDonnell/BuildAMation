// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QMakeBuilder package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder : Opus.Core.IBuilder
    {
        private string DisableQtPriPathName
        {
            get;
            set;
        }

        public static string GetProFilePath(Opus.Core.DependencyNode node)
        {
            string proFileDirectory = node.GetModuleBuildDirectory();
            string proFilePath = System.IO.Path.Combine(proFileDirectory, System.String.Format("{0}.pro", node.ModuleName));
            Opus.Core.Log.DebugMessage("QMake Pro File for node '{0}': '{1}'", node.UniqueModuleName, proFilePath);
            return proFilePath;
        }

        public static string GetQtConfiguration(Opus.Core.Target target)
        {
            if (!target.HasConfiguration(Opus.Core.EConfiguration.Debug) && !target.HasConfiguration(Opus.Core.EConfiguration.Optimized))
            {
                throw new Opus.Core.Exception("QMake only supports debug and optimized configurations");
            }
            string QMakeConfiguration = target.HasConfiguration(Opus.Core.EConfiguration.Debug) ? "debug" : "release";
            return QMakeConfiguration;
        }
    }
}