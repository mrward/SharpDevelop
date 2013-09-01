﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Management.Automation;
using ICSharpCode.PackageManagement.Scripting;
using ICSharpCode.SharpDevelop.Project;
using NuGet;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet(VerbsLifecycle.Install, "Package", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	public class InstallPackageCmdlet : PackageManagementCmdlet
	{
		public InstallPackageCmdlet()
			: this(
				PackageManagementServices.ConsoleHost,
				null)
		{
		}
		
		public InstallPackageCmdlet(
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base(consoleHost, terminatingError)
		{
		}
		
		[Parameter(Position = 0, Mandatory = true)]
		public string Id { get; set; }
		
		[Parameter(Position = 1)]
		public string ProjectName { get; set; }
		
		[Parameter(Position = 2)]
		public SemanticVersion Version { get; set; }
		
		[Parameter(Position = 3)]
		public string Source { get; set; }
		
		[Parameter]
		public SwitchParameter IgnoreDependencies { get; set; }
		
		[Parameter, Alias("Prerelease")]
		public SwitchParameter IncludePrerelease { get; set; }
		
		[Parameter]
		public FileConflictAction FileConflictAction { get; set; }
		
		[Parameter]
		public string Solution { get; set; }
		
		protected override void ProcessRecord()
		{
			OpenSolution();
			ThrowErrorIfProjectNotOpen();
			using (IConsoleHostFileConflictResolver resolver = CreateFileConflictResolver()) {
				InstallPackage();
			}
		}
		
		IConsoleHostFileConflictResolver CreateFileConflictResolver()
		{
			return ConsoleHost.CreateFileConflictResolver(FileConflictAction);
		}
		
		void OpenSolution()
		{
			if (Solution != null) {
				OpenSolution(Solution);
			}
		}
		
		void InstallPackage()
		{
			IPackageManagementProject project = GetProject();
			using (project.SourceRepository.StartInstallOperation(Id)) {
				InstallPackageAction action = CreateInstallPackageTask(project);
				action.Execute();
			}
		}
		
		IPackageManagementProject GetProject()
		{
			return ConsoleHost.GetProject(Source, ProjectName);
		}
		
		InstallPackageAction CreateInstallPackageTask(IPackageManagementProject project)
		{
			InstallPackageAction action = project.CreateInstallPackageAction();
			action.PackageId = Id;
			action.PackageVersion = Version;
			action.IgnoreDependencies = IgnoreDependencies.IsPresent;
			action.AllowPrereleaseVersions = IncludePrerelease.IsPresent;
			action.PackageScriptRunner = this;
			return action;
		}
	}
}
