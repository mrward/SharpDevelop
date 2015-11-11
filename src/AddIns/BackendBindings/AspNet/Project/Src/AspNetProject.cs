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
using CSharpBinding;
using ICSharpCode.Core;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using OmniSharp.Models;

using DependenciesMessage = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.DependenciesMessage;
using FrameworkData = Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages.FrameworkData;

namespace ICSharpCode.AspNet
{
	public class AspNetProject : CSharpProject
	{
		DnxProject project;
		bool addingReferences;
		Dictionary<string, DependenciesMessage> dependencies = new Dictionary<string, DependenciesMessage>();
		Dictionary<string, List<string>> savedFileReferences = new Dictionary<string, List<string>>();
		Dictionary<string, List<string>> savedProjectReferences = new Dictionary<string, List<string>>();
		Dictionary<string, List<string>> preprocessorSymbols = new Dictionary<string, List<string>>();
		string currentCommand;
		DnxFramework defaultFramework;
		string currentConfiguration = "Debug";
		
		public AspNetProject(ProjectLoadInformation loadInformation)
			: base(loadInformation)
		{
		}

		public AspNetProject(ProjectCreateInformation info)
			: base(info)
		{
		}
		
		public override string Language {
			get { return "C#"; }
		}
		
		protected override ProjectBehavior GetOrCreateBehavior()
		{
			return new AspNetProjectBehavior(this, base.GetOrCreateBehavior());
		}
		
		void AddAssemblyReference(string fileName)
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
		
		void AddProjectReference(string fileName)
		{
			ProjectReferenceProjectItem projectItem = AddProjectReferenceWithoutFiringEvent(fileName);
			RaiseProjectItemAdded(projectItem);
		}
		
		ProjectReferenceProjectItem AddProjectReferenceWithoutFiringEvent(string fileName)
		{
			AspNetProject referencedProject = ParentSolution.FindProjectByProjectJsonFileName(fileName);
			if (referencedProject != null) {
				var projectItem = new ProjectReferenceProjectItem(this, referencedProject);
				Items.Add(projectItem);
				return projectItem;
			} else {
				LoggingService.DebugFormatted("Unable to find project by json filename '{0}'.", fileName);
			}
			return null;
		}
		
		public string CurrentFramework { get; private set; }
		
		public void UpdateReferences(OmniSharp.Dnx.FrameworkProject frameworkProject)
		{
			EnsureCurrentFrameworkDefined(frameworkProject);
			
			List<string> fileReferences = frameworkProject.FileReferences.Keys.ToList ();
			savedFileReferences[frameworkProject.Framework] = fileReferences;
			
			List<string> projectReferences = frameworkProject.ProjectReferences.Keys.ToList();
			savedProjectReferences[frameworkProject.Framework] = projectReferences;
		
			if (CurrentFramework != frameworkProject.Framework)
				return;
			
			try {
				addingReferences = true;
				RemoveExistingReferences();
				UpdateReferences(fileReferences);
				UpdateProjectReferences(projectReferences);
			} finally {
				addingReferences = false;
			}
		}
		
		void EnsureCurrentFrameworkDefined(OmniSharp.Dnx.FrameworkProject frameworkProject)
		{
			if (CurrentFramework == null) {
				CurrentFramework = frameworkProject.Project.ProjectsByFramework.Keys.FirstOrDefault();
			}
		}

		void RemoveExistingReferences()
		{
			Items.RemoveAll(item => item is ReferenceProjectItem);
		}
		
		void UpdateReferences(IEnumerable<string> references)
		{
			ReferenceProjectItem projectItem = null;
			foreach (string reference in references) {
				projectItem = AddAssemblyReferenceWithoutFiringEvent(reference);
			}
			
			if (projectItem != null) {
				RaiseProjectItemAdded(projectItem);
			}
		}
		
		void UpdateProjectReferences(IEnumerable<string> references)
		{
			ProjectReferenceProjectItem projectItem = null;
			
			foreach (string reference in references) {
				projectItem = AddProjectReferenceWithoutFiringEvent(reference);
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
			var settings = new CompilerSettings();
			settings.ConditionalSymbols.AddRange(GetPreprocessorSymbols());
			CompilerSettings = settings;
			return settings;
		}
		
		public void Update(DnxProject project)
		{
			this.project = project;
		}
		
		public string GetCurrentCommand()
		{
			if (project == null || project.Commands == null)
				return null;
			
			ValidateCurrentCommand();
			
			if (currentCommand == null)
				return project.Commands.Keys.FirstOrDefault();
			
			return currentCommand;
		}
		
		void ValidateCurrentCommand()
		{
			if (currentCommand == null)
				return;
			
			if (!project.Commands.ContainsKey(currentCommand)) {
				currentCommand = null;
			}
		}
		
		public override Task<bool> BuildAsync(ProjectBuildOptions options, IBuildFeedbackSink feedbackSink, IProgressMonitor progressMonitor)
		{
			if (options.Target == BuildTarget.Build || options.Target == BuildTarget.Rebuild) {
				var builder = new AspNetProjectBuilder(this, progressMonitor, feedbackSink);
				return builder.Build();
			}
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

		public event EventHandler PackageRestoreStarted;
		
		public void OnPackageRestoreStarted()
		{
			var handler = PackageRestoreStarted;
			if (handler != null)
				handler (this, new EventArgs());
		}

		public event EventHandler PackageRestoreFinished;

		public void OnPackageRestoreFinished()
		{
			var handler = PackageRestoreFinished;
			if (handler != null)
				handler (this, new EventArgs());
		}

		public IEnumerable<string> GetCommands()
		{
			if (project == null || project.Commands == null)
				return Enumerable.Empty<string>();
			
			return project.Commands.Keys.AsEnumerable();
		}
		
		public DnxFramework DefaultFramework {
			get { return defaultFramework; }
			set {
				defaultFramework = value;
				if (!IsCurrentFramework(value)) {
					UpdateCurrentFramework(value);
					
					try {
						addingReferences = true;
						RefreshReferences();
						RefreshProjectReferences();
						RefreshCompilerSettings();
					} finally {
						addingReferences = false;
					}
				}
			}
		}
		
		public IEnumerable<DnxFramework> GetFrameworks()
		{
			if (project == null || project.Frameworks == null)
				return Enumerable.Empty<DnxFramework>();
			
			return project.Frameworks;
		}

		public void UseCommand(string command)
		{
			currentCommand = command;
		}
		
		void RefreshReferences()
		{
			List<string> fileReferences = null;
			if (!savedFileReferences.TryGetValue(CurrentFramework, out fileReferences)) {
				LoggingService.WarnFormatted("Unable to find references for framework '{0}'.", CurrentFramework);
				return;
			}
			
			UpdateReferences(fileReferences);
		}
		
		void RefreshProjectReferences()
		{
			List<string> projectReferences = null;
			if (!savedProjectReferences.TryGetValue(CurrentFramework, out projectReferences)) {
				LoggingService.WarnFormatted("Unable to find project for framework '{0}'.", CurrentFramework);
				return;
			}

			UpdateProjectReferences(projectReferences);
		}
		
		bool IsCurrentFramework(DnxFramework framework)
		{
			if (framework == null) {
				return CurrentFramework == savedFileReferences.Keys.FirstOrDefault();
			}
			
			return framework.Name == CurrentFramework;
		}
		
		void UpdateCurrentFramework(DnxFramework framework)
		{
			if (framework == null) {
				CurrentFramework = savedFileReferences.Keys.FirstOrDefault();
			} else {
				CurrentFramework = framework.Name;
			}
		}
		
		public void UpdateParseOptions(OmniSharp.Dnx.FrameworkProject frameworkProject, Microsoft.CodeAnalysis.ParseOptions options)
		{
			EnsureCurrentFrameworkDefined(frameworkProject);

			List<string> symbols = options.PreprocessorSymbolNames.ToList();
			preprocessorSymbols[frameworkProject.Framework] = symbols;

			if (CurrentFramework != frameworkProject.Framework)
				return;

			RefreshCompilerSettings();
		}
		
		void RefreshCompilerSettings()
		{
			OnPropertyChanged(new ProjectPropertyChangedEventArgs("DefineConstants"));
		}

		IEnumerable<string> GetPreprocessorSymbols()
		{
			if (CurrentFramework == null)
				return Enumerable.Empty<string>();
			
			List<string> symbols = null;
			if (!preprocessorSymbols.TryGetValue(CurrentFramework, out symbols)) {
				LoggingService.WarnFormatted("Unable to find preprocessor symbols for framework '{0}'.", CurrentFramework);
				return Enumerable.Empty<string>();
			}
			
			return symbols;
		}
		
		protected override void OnActiveConfigurationChanged(EventArgs e)
		{
			if (currentConfiguration != ActiveConfiguration.Configuration) {
				currentConfiguration = ActiveConfiguration.Configuration;
				AspNetServices.ProjectService.ChangeConfiguration(currentConfiguration);
			}
			base.OnActiveConfigurationChanged(e);
		}
		
		public string JsonPath {
			get {
				if (project != null) {
					return project.Path;
				}
				return null;
			}
		}
		
		public void OnReferenceAddedToProject(ReferenceProjectItem projectItem)
		{
			if (addingReferences)
				return;

			if (projectItem.ItemType == ItemType.ProjectReference) {
				var jsonFile = ProjectJsonFile.Read(this);
				if (jsonFile.Exists) {
					jsonFile.AddProjectReference((ProjectReferenceProjectItem)projectItem);
					jsonFile.Save();
				} else {
					LoggingService.DebugFormatted("Unable to find project.json '{0}'", jsonFile.Path);
				}
			}
		}

		public void OnReferenceRemovedFromProject(ReferenceProjectItem projectItem)
		{
			if (addingReferences)
				return;

			if (projectItem.ItemType == ItemType.ProjectReference) {
				var jsonFile = ProjectJsonFile.Read(this);
				if (jsonFile.Exists) {
					jsonFile.RemoveProjectReference((ProjectReferenceProjectItem)projectItem);
					jsonFile.Save();
				} else {
					LoggingService.DebugFormatted("Unable to find project.json '{0}'", jsonFile.Path);
				}
			}
		}
	}
}
