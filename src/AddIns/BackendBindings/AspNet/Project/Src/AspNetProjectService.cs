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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.AspNet.Omnisharp.SharpDevelop;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;
using OmniSharp.Dnx;
using OmniSharp.Models;

namespace ICSharpCode.AspNet
{
	public class AspNetProjectService
	{
		DnxContext context;
		DnxProjectSystem projectSystem;
		SharpDevelopApplicationLifetime applicationLifetime;
		ISolution solution;
		string initializeError = String.Empty;
		ConcurrentDictionary<string, AspNetProjectBuilder> builders =
			new ConcurrentDictionary<string, AspNetProjectBuilder>();

		public AspNetProjectService()
		{
			SD.ProjectService.SolutionOpened += SolutionOpened;
			SD.ProjectService.SolutionClosed += SolutionClosed;
			SD.ProjectService.ProjectItemAdded += ProjectItemAdded;
			SD.ProjectService.ProjectItemRemoved += ProjectItemRemoved;
			FileUtility.FileLoaded += FileLoaded;
			FileUtility.FileSaved += FileChanged;
		}
		
		void SolutionClosed(object sender, SolutionEventArgs e)
		{
			UnloadProjectSystem();
		}
		
		void UnloadProjectSystem()
		{
			if (applicationLifetime != null) {
				applicationLifetime.Stopping();
				applicationLifetime.Dispose();
				applicationLifetime = null;
				if (projectSystem != null) {
					projectSystem.Dispose ();
					projectSystem = null;
				}
				context = null;
				solution.Projects.CollectionChanged -= ProjectsChanged;
				solution = null;
			}
		}

		void TryLoadAspNetProjectSystem(ISolution solution)
		{
			try {
				LoadAspNetProjectSystem(solution);
			} catch (Exception ex) {
				LoggingService.WarnFormatted("DNX project system initialize failed. {0}", ex);
				UnloadProjectSystem();
				initializeError = "Unable to initialize DNX project system. " + ex.Message;
			}
		}
		
		void SolutionOpened(object sender, SolutionEventArgs e)
		{
			try {
				if (e.Solution.HasAspNetProjects()) {
					e.Solution.Projects.CollectionChanged += ProjectsChanged;
					LoadAspNetProjectSystem(e.Solution);
				}
			} catch (Exception ex) {
				LoggingService.WarnFormatted("DNX project system initialize failed. {0}", ex);
				UnloadProjectSystem();
				initializeError = "Unable to initialize DNX project system. " + ex.Message;
			}
		}
		
		void LoadAspNetProjectSystem(ISolution solution)
		{
			this.solution = solution;
			applicationLifetime = new SharpDevelopApplicationLifetime();
			context = new DnxContext();
			var factory = new DnxProjectSystemFactory();
			projectSystem = factory.CreateProjectSystem(solution, applicationLifetime, context);
			projectSystem.Initalize();
			
			if (context.RuntimePath == null) {
				string error = GetRuntimeError(projectSystem);
				throw new ApplicationException(error);
			}
		}
		
		static string GetRuntimeError(DnxProjectSystem projectSystem)
		{
			if (projectSystem.DnxPaths != null &&
				projectSystem.DnxPaths.RuntimePath != null &&
				projectSystem.DnxPaths.RuntimePath.Error != null) {
				return projectSystem.DnxPaths.RuntimePath.Error.Text;
			}
			return "Unable to find DNX runtime.";
 		}
		
		public bool HasCurrentDnxRuntime {
			get { return context != null; }
		}

		public string CurrentRuntimeError {
			get { return initializeError; }
		}
		
		public void OnReferencesUpdated(ProjectId projectId, FrameworkProject frameworkProject)
		{
			SD.MainThread.InvokeAsyncAndForget(() => {
				var locator = new AspNetProjectLocator(context);
				AspNetProject project = locator.FindProject(projectId);
				if (project != null) {
					project.UpdateReferences(frameworkProject);
				}
			});
		}
		
		public void OnProjectChanged(DnxProject project)
		{
			SD.MainThread.InvokeAsyncAndForget(() => UpdateProject(project));
		}
		
		void UpdateProject(DnxProject project)
		{
			AspNetProject matchedProject = FindProjectByProjectJsonFileName(project.Path);
			if (matchedProject != null) {
				matchedProject.Update(project);
			}
		}
		
		public ProcessStartInfo GetProcessStartInfo(AspNetProject project)
		{
			var startInfo = new DnxRuntimeProcessStartInfo(context.RuntimePath);
			return startInfo.GetProcessStartInfo(project);
		}
		
		public void DependenciesUpdated(OmniSharp.Dnx.Project project, DependenciesMessage message)
		{
			SD.MainThread.InvokeAsyncAndForget(() => UpdateDependencies(project, message));
		}

		static AspNetProject FindProjectByProjectJsonFileName(string fileName)
		{
			ISolution solution = SD.ProjectService.CurrentSolution;
			if (solution == null)
				return null;

			AspNetProject project = solution.FindProjectByProjectJsonFileName(fileName);
			if (project != null) {
				return project;
			}
			
			LoggingService.WarnFormatted("Unable to find project by json file. '{0}'", fileName);
			return null;
		}

		void UpdateDependencies(OmniSharp.Dnx.Project project, DependenciesMessage message)
		{
			AspNetProject matchedProject = FindProjectByProjectJsonFileName(project.Path);
			if (matchedProject != null) {
				matchedProject.UpdateDependencies(message);
			}
		}
		
		public void PackageRestoreStarted(string projectJsonFileName)
		{
			SD.MainThread.InvokeAsyncAndForget(() => OnPackageRestoreStarted(projectJsonFileName));
		}

		void OnPackageRestoreStarted(string projectJsonFileName)
		{
			AspNetProject matchedProject = FindProjectByProjectJsonFileName(projectJsonFileName);
			if (matchedProject != null) {
				matchedProject.OnPackageRestoreStarted();
			}
		}

		public void PackageRestoreFinished(string projectJsonFileName)
		{
			SD.MainThread.InvokeAsyncAndForget(() => OnPackageRestoreFinished(projectJsonFileName));
		}

		void OnPackageRestoreFinished(string projectJsonFileName)
		{
			AspNetProject matchedProject = FindProjectByProjectJsonFileName(projectJsonFileName);
			if (matchedProject != null) {
				matchedProject.OnPackageRestoreFinished();
			}
		}
		
		void ProjectsChanged(IReadOnlyCollection<IProject> removedItems, IReadOnlyCollection<IProject> addedItems)
		{
			SD.MainThread.InvokeAsyncAndForget(ReloadProjectSystem);
		}
		
		void ReloadProjectSystem()
		{
			ISolution currentSolution = solution;
			UnloadProjectSystem();
			TryLoadAspNetProjectSystem(currentSolution);
		}

		public AspNetProject GetStartupDnxProject()
		{
			if (solution == null)
				return null;
			
			return solution.StartupProject as AspNetProject;
		}
		
		public void OnParseOptionsChanged(ProjectId projectId, ParseOptions options)
		{
			SD.MainThread.InvokeAsyncAndForget (()  => {
				var locator = new AspNetProjectLocator(context);
				AspNetProject project = locator.FindProject(projectId);
				if (project != null) {
					project.UpdateParseOptions(locator.FrameworkProject, options);
				}
			});
		}
		
		public void ChangeConfiguration(string config)
		{
			if (projectSystem != null) {
				projectSystem.ChangeConfiguration(config);
			}
		}
		
		public void GetDiagnostics(AspNetProjectBuilder builder)
		{
			builders.TryAdd(builder.ProjectPath, builder);
			projectSystem.GetDiagnostics(builder.ProjectPath);
		}
		
		public void ReportDiagnostics(OmniSharp.Dnx.Project project, DiagnosticsMessage[] messages)
		{
			AspNetProjectBuilder builder = null;
			if (builders.TryRemove(project.Path, out builder)) {
				builder.OnDiagnostics(messages);
			} else {
				LoggingService.WarnFormatted("Unable to find builder for project '{0}'", project.Path);
			}
		}
		
		void ProjectItemAdded(object sender, ProjectItemEventArgs e)
		{
			var project = e.Project as AspNetProject;
			if (project != null && e.ProjectItem is ReferenceProjectItem) {
				project.OnReferenceAddedToProject((ReferenceProjectItem)e.ProjectItem);
			}
		}

		void ProjectItemRemoved(object sender, ProjectItemEventArgs e)
		{
			var project = e.Project as AspNetProject;
			if (project != null && e.ProjectItem is ReferenceProjectItem) {
				project.OnReferenceRemovedFromProject((ReferenceProjectItem)e.ProjectItem);
			}
		}
		
		void FileLoaded(object sender, FileLoadEventArgs e)
		{
			if (e.IsReload) {
				FileChanged(sender, e);
			}
		}
		
		void FileChanged(object sender, FileNameEventArgs e)
		{
			if (solution == null)
				return;

			if (!solution.HasAspNetProjects())
				return;

			if (!IsGlobalJsonFileChanged(e.FileName))
				return;

			TryLoadAspNetProjectSystem(solution);
		}

		static bool IsGlobalJsonFileChanged(FileName fileName)
		{
			string name = fileName.GetFileName();
			if (name != null) {
				return name.Equals("global.json", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
	}
}
