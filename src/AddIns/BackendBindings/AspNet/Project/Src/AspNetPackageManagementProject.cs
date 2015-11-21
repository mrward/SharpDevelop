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
using System.Linq;
using System.Runtime.Versioning;
using ICSharpCode.PackageManagement;
using NuGet;

namespace ICSharpCode.AspNet
{
	public class AspNetPackageManagementProject : IPackageManagementProject
	{
		AspNetProject project;
		FrameworkName targetFramework;
		IPackageManagementEvents packageManagementEvents;
		
		public AspNetPackageManagementProject(IPackageRepository sourceRepository, AspNetProject project)
		{
			SourceRepository = sourceRepository;
			this.project = project;
			packageManagementEvents = PackageManagementServices.PackageManagementEvents;
			targetFramework = new FrameworkName("DNX", new Version("5.0"));
		}

		public event EventHandler<PackageOperationEventArgs> PackageInstalled;

		public event EventHandler<PackageOperationEventArgs> PackageUninstalled;

		public event EventHandler<PackageOperationEventArgs> PackageReferenceAdded;

		public event EventHandler<PackageOperationEventArgs> PackageReferenceRemoving;

		public ICSharpCode.PackageManagement.EnvDTE.Project ConvertToDTEProject()
		{
			throw new NotImplementedException();
		}

		public bool IsPackageInstalled(IPackage package)
		{
			return false;
		}

		public bool IsPackageInstalled(string packageId)
		{
			return false;
		}

		public bool HasOlderPackageInstalled(IPackage package)
		{
			return false;
		}

		public IQueryable<IPackage> GetPackages()
		{
			return new IPackage[0].AsQueryable();
		}

		public IEnumerable<IPackage> GetPackagesInReverseDependencyOrder()
		{
			return GetPackages();
		}

		public IEnumerable<PackageOperation> GetInstallPackageOperations(IPackage package, InstallPackageAction installAction)
		{
			return new PackageOperation[0];
		}

		public IEnumerable<PackageOperation> GetUpdatePackagesOperations(IEnumerable<IPackage> packages, IUpdatePackageSettings settings)
		{
			return new PackageOperation[0];
		}

		public void InstallPackage(IPackage package, InstallPackageAction installAction)
		{
			project.AddNuGetPackage(package);
		}

		public void UpdatePackage(IPackage package, UpdatePackageAction updateAction)
		{
			throw new NotImplementedException();
		}

		public void UninstallPackage(IPackage package, UninstallPackageAction uninstallAction)
		{
			throw new NotImplementedException();
		}

		public void UpdatePackages(UpdatePackagesAction action)
		{
			throw new NotImplementedException();
		}

		public void UpdatePackageReference(IPackage package, IUpdatePackageSettings settings)
		{
			throw new NotImplementedException();
		}

		public InstallPackageAction CreateInstallPackageAction()
		{
			return new InstallPackageAction(this, packageManagementEvents);
		}

		public UninstallPackageAction CreateUninstallPackageAction()
		{
			throw new NotImplementedException();
		}

		public UpdatePackageAction CreateUpdatePackageAction()
		{
			throw new NotImplementedException();
		}

		public UpdatePackagesAction CreateUpdatePackagesAction()
		{
			throw new NotImplementedException();
		}

		public void RunPackageOperations(IEnumerable<PackageOperation> expectedOperations)
		{
			throw new NotImplementedException();
		}

		public string Name {
			get { return project.Name; }
		}

		public FrameworkName TargetFramework {
			get { return targetFramework; }
		}

		public ILogger Logger {
			get { return NullLogger.Instance; }
			set { }
		}

		public IPackageRepository SourceRepository { get; private set; }

		public IPackageConstraintProvider ConstraintProvider {
			get { return NullConstraintProvider.Instance; }
		}
	}
}
