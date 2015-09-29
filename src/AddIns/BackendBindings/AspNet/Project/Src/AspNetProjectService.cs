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

		public AspNetProjectService()
		{
			SD.ProjectService.SolutionOpened += SolutionOpened;
			SD.ProjectService.SolutionClosed += SolutionClosed;
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
				projectSystem.Dispose ();
				projectSystem = null;
				context = null;
				
				solution.Projects.CollectionChanged -= ProjectsChanged;
				solution = null;
			}
		}
		
		void SolutionOpened(object sender, SolutionEventArgs e)
		{
			try {
				if (e.Solution.HasAspNetProjects()) {
					LoadAspNetProjectSystem(e.Solution);
					solution.Projects.CollectionChanged += ProjectsChanged;
				}
			} catch (Exception ex) {
				MessageService.ShowError(ex.Message);
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
		}
		
		public void OnReferencesUpdated(ProjectId projectId, FrameworkProject frameworkProject)
		{
			SD.MainThread.InvokeAsyncAndForget(() => {
				var maintainer = new AspNetProjectReferenceMaintainer(context);
				maintainer.UpdateReferences(projectId, frameworkProject);
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
		
		public ProcessStartInfo GetProcessStartInfo(DirectoryName directory, string command)
		{
			var startInfo = new DnxRuntimeProcessStartInfo(context.RuntimePath);
			return startInfo.GetProcessStartInfo(directory, command);
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
			LoadAspNetProjectSystem(currentSolution);
		}

		public AspNetProject GetStartupDnxProject()
		{
			if (solution == null)
				return null;
			
			return solution.StartupProject as AspNetProject;
		}
	}
}
