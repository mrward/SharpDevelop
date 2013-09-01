﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using NuGet;

namespace ICSharpCode.PackageManagement
{
	public class UpdatedPackageViewModel : PackageViewModel
	{
		public UpdatedPackageViewModel(
			IPackageViewModelParent parent,
			IPackageFromRepository package,
			SelectedProjectsForUpdatedPackages selectedProjects,
			IPackageManagementEvents packageManagementEvents,
			IPackageActionRunner actionRunner,
			ILogger logger)
			: base(parent, package, selectedProjects, packageManagementEvents, actionRunner, logger)
		{
		}
		
		protected override ProcessPackageOperationsAction CreateInstallPackageAction(
			IPackageManagementProject project)
		{
			return project.CreateUpdatePackageAction();
		}
		
		protected override IDisposable StartInstallOperation(IPackageFromRepository package)
		{
			return package.StartUpdateOperation();
		}
	}
}
