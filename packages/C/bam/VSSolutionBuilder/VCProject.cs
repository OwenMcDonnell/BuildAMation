#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace VSSolutionBuilder
{
    public class VCProject :
        ICProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid = System.Guid.NewGuid();
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private ProjectFileCollection HeaderFileCollection = new ProjectFileCollection();
        private ProjectFileCollection ResourceFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();
        private Bam.Core.UniqueList<string> ReferencesList = new Bam.Core.UniqueList<string>();

#if true
#else
        public
        VCProject(
            string moduleName,
            string projectPathName,
            Bam.Core.PackageIdentifier packageId,
            Bam.Core.ProxyModulePath proxyPath)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageId.Path;
            if (null != proxyPath)
            {
                this.PackageDirectory = proxyPath.Combine(packageId.Location).AbsolutePath;
            }

            if (this.PackageDirectory[this.PackageDirectory.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                this.PackageUri = new System.Uri(this.PackageDirectory, System.UriKind.Absolute);
            }
            else
            {
                this.PackageUri = new System.Uri(this.PackageDirectory + System.IO.Path.DirectorySeparatorChar, System.UriKind.Absolute);
            }

            this.ProjectGuid = new DeterministicGuid(this.PathName).Guid;
        }
#endif

        string IProject.Name
        {
            get
            {
                return this.ProjectName;
            }
        }

        string IProject.PathName
        {
            get
            {
                return this.PathName;
            }
        }

        System.Guid IProject.Guid
        {
            get
            {
                return this.ProjectGuid;
            }
        }

        public string PackageDirectory
        {
            get;
            private set;
        }

        System.Collections.Generic.List<string> IProject.Platforms
        {
            get
            {
                return this.PlatformList;
            }
        }

        ProjectConfigurationCollection IProject.Configurations
        {
            get
            {
                return this.ProjectConfigurations;
            }
        }

        ProjectFileCollection IProject.SourceFiles
        {
            get
            {
                return this.SourceFileCollection;
            }
        }

        ProjectFileCollection ICProject.HeaderFiles
        {
            get
            {
                return this.HeaderFileCollection;
            }
        }

        ProjectFileCollection ICProject.ResourceFiles
        {
            get
            {
                return this.ResourceFileCollection;
            }
        }

        System.Collections.Generic.List<IProject> IProject.DependentProjects
        {
            get
            {
                return this.DependentProjectList;
            }
        }

        Bam.Core.UniqueList<string> IProject.References
        {
            get
            {
                return this.ReferencesList;
            }
        }

        public string GroupName
        {
            get;
            set;
        }

        void
        IProject.Serialize()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                var projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by BuildAMation v" + Bam.Core.State.VersionString));
                var vsProjectElement = xmlDocument.CreateElement("VisualStudioProject");

                // preamble
                vsProjectElement.SetAttribute("ProjectType", "Visual C++");
                {
                    var solutionType = Bam.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var ProjectVersionProperty = solutionType.GetProperty("ProjectVersion");
                    vsProjectElement.SetAttribute("Version", ProjectVersionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string);
                    SolutionInstance = null;
                }
                vsProjectElement.SetAttribute("Name", this.ProjectName);
                vsProjectElement.SetAttribute("ProjectGUID", this.ProjectGuid.ToString("B").ToUpper());

                // platforms
                var platformsElement = xmlDocument.CreateElement("Platforms");
                foreach (var platform in this.PlatformList)
                {
                    var platformElement = xmlDocument.CreateElement("Platform");
                    platformElement.SetAttribute("Name", platform);
                    platformsElement.AppendChild(platformElement);
                }
                vsProjectElement.AppendChild(platformsElement);

                // tool files
                // TODO

                // configurations
                var configurationsElement = xmlDocument.CreateElement("Configurations");
                // TODO: convert to using 'var'
                foreach (ProjectConfiguration configuration in this.ProjectConfigurations)
                {
                    configurationsElement.AppendChild(configuration.Serialize(xmlDocument, projectLocationUri));
                }
                vsProjectElement.AppendChild(configurationsElement);

                // files
                var filesElement = xmlDocument.CreateElement("Files");
                if (this.SourceFileCollection.Count > 0)
                {
                    filesElement.AppendChild(this.SourceFileCollection.Serialize(xmlDocument, "Source Files", projectLocationUri, this.PackageUri));
                }

                if (this.HeaderFileCollection.Count > 0)
                {
                    filesElement.AppendChild(this.HeaderFileCollection.Serialize(xmlDocument, "Header Files", projectLocationUri, this.PackageUri));
                }

                if (this.ResourceFileCollection.Count > 0)
                {
                    filesElement.AppendChild(this.ResourceFileCollection.Serialize(xmlDocument, "Resource Files", projectLocationUri, this.PackageUri));
                }

                vsProjectElement.AppendChild(filesElement);

                xmlDocument.AppendChild(vsProjectElement);
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(exception, message);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = true;

            try
            {
                using (var xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                    xmlWriter.WriteWhitespace(xmlWriterSettings.NewLineChars);
                }
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(exception, message);
            }
        }

        VisualStudioProcessor.EVisualStudioTarget IProject.VSTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.VCPROJ;
            }
        }
    }
}