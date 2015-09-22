// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Templates
{
	internal class SolutionDescriptor
	{
		SolutionFolderDescriptor mainFolder = new SolutionFolderDescriptor("");
		List<FileDescriptionTemplate> files = new List<FileDescriptionTemplate>();
		
		class SolutionFolderDescriptor
		{
			internal string name;
			internal List<ProjectDescriptor> projectDescriptors = new List<ProjectDescriptor>();
			internal List<SolutionFolderDescriptor> solutionFoldersDescriptors = new List<SolutionFolderDescriptor>();
			internal List<SolutionFileItemDescriptor> solutionFileItemDescriptors = new List<SolutionFileItemDescriptor>();
			
			internal void Read(XmlElement element, IReadOnlyFileSystem fileSystem)
			{
				name = element.GetAttribute("name");
				foreach (XmlNode node in element.ChildNodes) {
					switch (node.Name) {
						case "Project":
							projectDescriptors.Add(new ProjectDescriptor((XmlElement)node, fileSystem));
							break;
						case "SolutionFolder":
							solutionFoldersDescriptors.Add(new SolutionFolderDescriptor((XmlElement)node, fileSystem));
							break;
						case "SolutionFileItems":
							LoadSolutionFileItems((XmlElement)node);
							break;
					}
				}
			}
			
			ISolutionFolder GetOrCreateFolder(ISolutionFolder parentFolder, string folderName)
			{
				SolutionFolder existingFolder = parentFolder.Items
					.OfType<SolutionFolder>()
					.FirstOrDefault(item => item.Name == folderName);
				
				if (existingFolder != null)
					return existingFolder;
				
				return parentFolder.CreateFolder(folderName);
			}
			
			internal bool AddContents(ISolutionFolder parentFolder, ProjectTemplateResult templateResult, string defaultLanguage)
			{
				if (!CreateSolutionFilesItems(parentFolder))
					return false;
				
				// Create sub projects
				foreach (SolutionFolderDescriptor folderDescriptor in solutionFoldersDescriptors) {
					ISolutionFolder folder = GetOrCreateFolder(parentFolder, folderDescriptor.name);
					if (!folderDescriptor.AddContents(folder, templateResult, defaultLanguage))
						return false;
				}
				foreach (ProjectDescriptor projectDescriptor in projectDescriptors) {
					bool success = projectDescriptor.CreateProject(templateResult, defaultLanguage, parentFolder);
					if (!success)
						return false;
				}
				
				return true;
			}
			
			void LoadSolutionFileItems(XmlElement filesElement)
			{
				foreach (XmlElement fileElement in filesElement.ChildNodes.OfType<XmlElement>()) {
					solutionFileItemDescriptors.Add(new SolutionFileItemDescriptor(fileElement));
				}
			}
			
			bool CreateSolutionFilesItems(ISolutionFolder parentFolder)
			{
				foreach (SolutionFileItemDescriptor descriptor in solutionFileItemDescriptors) {
					if (!descriptor.CreateSolutionFileItem(parentFolder))
						return false;
				}
				return true;
			}
			
			public SolutionFolderDescriptor(XmlElement element, IReadOnlyFileSystem fileSystem)
			{
				Read(element, fileSystem);
			}
			
			public SolutionFolderDescriptor(string name)
			{
				this.name = name;
			}
		}
		
		string name;
		string startupProject    = null;
		
		#region public properties
		public string StartupProject {
			get {
				return startupProject;
			}
		}

		public List<ProjectDescriptor> ProjectDescriptors {
			get {
				return mainFolder.projectDescriptors;
			}
		}
		#endregion

		protected SolutionDescriptor(string name)
		{
			this.name = name;
		}
		
		internal bool AddContents(ISolutionFolder parentFolder, ProjectTemplateResult templateResult, string defaultLanguage)
		{
			CreateFiles(templateResult);
			return mainFolder.AddContents(parentFolder, templateResult, defaultLanguage);
		}
		
		public static SolutionDescriptor CreateSolutionDescriptor(XmlElement element, IReadOnlyFileSystem fileSystem)
		{
			SolutionDescriptor solutionDescriptor = new SolutionDescriptor(element.Attributes["name"].InnerText);
			
			if (element["Options"] != null && element["Options"]["StartupProject"] != null) {
				solutionDescriptor.startupProject = element["Options"]["StartupProject"].InnerText;
			}
			if (element["Files"] != null) {
				solutionDescriptor.files = LoadFiles(element["Files"], fileSystem).ToList();
			}
			
			solutionDescriptor.mainFolder.Read(element, fileSystem);
			return solutionDescriptor;
		}
		
		static IEnumerable<FileDescriptionTemplate> LoadFiles(XmlElement filesElement, IReadOnlyFileSystem fileSystem)
		{
			foreach (XmlElement fileElement in filesElement.ChildNodes.OfType<XmlElement>()) {
				yield return new FileDescriptionTemplate(fileElement, fileSystem);
			}
		}
		
		void CreateFiles(ProjectTemplateResult templateResult)
		{
			var generator = new TemplateFileGenerator(files, templateResult.Options.Solution.Directory) {
				ProjectName = templateResult.Options.ProjectName,
				SolutionName = templateResult.Options.SolutionName,
				UserDefinedProjectName = templateResult.Options.ProjectName,
				OverwriteFiles = false
			};
			generator.GenerateFiles();
		}
	}
}
