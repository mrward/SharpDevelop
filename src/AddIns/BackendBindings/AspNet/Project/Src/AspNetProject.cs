// Copyright (c) 2015 AlphaSierraPapa for the SharpDevelop Team
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Core;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using OmniSharp.Models;

using DependenciesMessage = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.DependenciesMessage;
using FrameworkData = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.FrameworkData;

namespace ICSharpCode.AspNet
{
	public class AspNetProject : CompilableProject
	{
		AspNet5Project project;
		Dictionary<string, DependenciesMessage> dependencies = new Dictionary<string, DependenciesMessage>();
		
		public AspNetProject(ProjectLoadInformation loadInformation)
			: base(loadInformation)
		{
			if (loadInformation.InitializeTypeSystem)
				InitializeProjectContent(new CSharpProjectContent());
		}

		public AspNetProject(ProjectCreateInformation info)
			: base(info)
		{
			if (info.InitializeTypeSystem)
				InitializeProjectContent(new CSharpProjectContent());
		}
		
		public override string Language {
			get { return "C#"; }
		}
		
		protected override ProjectBehavior GetOrCreateBehavior()
		{
			return new AspNetProjectBehavior(this, base.GetOrCreateBehavior());
		}
		
		public void AddAssemblyReference(string fileName)
		{
			ReferenceProjectItem projectItem = AddAssemblyReferenceWithoutFiringEvent(fileName);
			RaiseProjectItemAdded(projectItem);
		}
		
		ReferenceProjectItem AddAssemblyReferenceWithoutFiringEvent(string fileName)
		{
			var projectItem = new ReferenceProjectItem(this) {
				FileName = new FileName(fileName),
				Include = Path.GetFileNameWithoutExtension(fileName)
			};
			Items.Add(projectItem);
			return projectItem;
		}
		
		void RaiseProjectItemAdded(ProjectItem projectItem)
		{
			IProjectServiceRaiseEvents projectEvents = SD.GetService<IProjectServiceRaiseEvents>();
			if (projectEvents != null) {
				projectEvents.RaiseProjectItemAdded(new ProjectItemEventArgs(this, projectItem));
			}
		}
		
		public bool IsCurrentFramework(string framework, IEnumerable<string> frameworks)
		{
			if (CurrentFramework == null) {
				CurrentFramework = frameworks.FirstOrDefault();
			}
			
			return CurrentFramework == framework;
		}
		
		public string CurrentFramework { get; private set; }

		public void UpdateReferences(IEnumerable<string> references)
		{
			Items.RemoveAll(item => item is ReferenceProjectItem);
			
			ReferenceProjectItem projectItem = null;
			foreach (string reference in references) {
				projectItem = AddAssemblyReferenceWithoutFiringEvent(reference);
			}
			
			if (projectItem != null) {
				RaiseProjectItemAdded(projectItem);
			}
		}

		public void LoadFiles()
		{
			foreach (string fileName in System.IO.Directory.GetFiles(Directory, "*.*", SearchOption.AllDirectories)) {
				if (IsSupportedProjectFileItem(fileName)) {
					Items.Add(CreateFileProjectItem(fileName));
				}
			}
		}

		bool IsSupportedProjectFileItem(string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension.EndsWith("proj", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)) {
				return false;
			} else if (extension.Equals(".user", StringComparison.OrdinalIgnoreCase)) {
				return false;
			}
			return true;
		}

		FileProjectItem CreateFileProjectItem(string fileName)
		{
			return new FileProjectItem(this, GetDefaultItemType(fileName)) {
				FileName = new FileName(fileName)
			};
		}
		
		public override ItemType GetDefaultItemType(string fileName)
		{
			if (IsCSharpFile(fileName)) {
				return ItemType.Compile;
			}
			return base.GetDefaultItemType(fileName);
		}
		
		static bool IsCSharpFile(string fileName)
		{
			string extension = Path.GetExtension(fileName);
			return String.Equals(".cs", extension, StringComparison.OrdinalIgnoreCase);
		}
		
		public override void Save(string fileName)
		{
		}
		
		protected override object CreateCompilerSettings()
		{
			return new CompilerSettings();
		}
		
		public void Update(AspNet5Project project)
		{
			this.project = project;
		}
		
		public string GetCurrentCommand()
		{
			if (project == null)
				return null;
			
			return project.Commands.Keys.FirstOrDefault();
		}
		
		public override Task<bool> BuildAsync(ProjectBuildOptions options, IBuildFeedbackSink feedbackSink, IProgressMonitor progressMonitor)
		{
			return Task.FromResult(true);
		}

		public void UpdateDependencies(DependenciesMessage message)
		{
			dependencies[message.Framework.FrameworkName] = message;
			OnDependenciesChanged();
		}

		public event EventHandler DependenciesChanged;

		protected virtual void OnDependenciesChanged()
		{
			var handler = DependenciesChanged;
			if (handler != null)
				handler (this, new EventArgs());
		}

		public bool HasDependencies()
		{
			return dependencies.Any();
		}

		public IEnumerable<DependenciesMessage> GetDependencies()
		{
			foreach (DependenciesMessage message in dependencies.Values) {
				yield return message;
			}
		}
	}
}
