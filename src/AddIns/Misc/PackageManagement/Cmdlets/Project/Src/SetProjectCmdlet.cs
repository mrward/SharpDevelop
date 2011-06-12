// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Management.Automation;
using ICSharpCode.PackageManagement.Scripting;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet(VerbsCommon.Set, "Project", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	public class SetProjectCmdlet : PackageManagementCmdlet
	{
		public SetProjectCmdlet()
			: this(
				PackageManagementServices.ConsoleHost,
				null)
		{
		}
		
		public SetProjectCmdlet(
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base(consoleHost, terminatingError)
		{
		}
		
		[Parameter(Position = 0, Mandatory = true)]
		public string Name { get; set; }

		[Parameter(Position = 1, Mandatory = true)]
		public string Solution { get; set; }
		
		protected override void ProcessRecord()
		{
			OpenSolution();
			IProject project = ConsoleHost.Solution.GetMSBuildProject(Name);
			if (project == null) {
				throw new ApplicationException("Unknown project: " + Name);
			}
			ConsoleHost.DefaultProject = project;
		}
		
		void OpenSolution()
		{
			if (Solution != null) {
				ConsoleHost.OpenSolution(Solution);
			}
		}
	}
}
