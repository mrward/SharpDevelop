// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.IO;
using System.Resources;

using ICSharpCode.Core;
using ICSharpCode.PackageManagement.Scripting;

namespace ICSharpCode.PackageManagement
{
	public static class PackageManagementServices
	{
		static readonly PackageManagementOptions options;
		static readonly PackageManagementSolution solution;
		static readonly PackageManagementConsoleHostProvider consoleHostProvider;
		static readonly RegisteredPackageRepositories registeredPackageRepositories;
		static readonly PackageManagementEvents packageManagementEvents = new PackageManagementEvents();
		static readonly PackageManagementProjectService projectService = new PackageManagementProjectService();
		static readonly ProjectBrowserRefresher projectBrowserRefresher;
		static readonly PackageManagementOutputMessagesView outputMessagesView;
		static readonly RunPackageInitializationScriptsOnSolutionOpen runPackageInitializationScripts;
		static readonly ResetPowerShellWorkingDirectoryOnSolutionClosed resetPowerShellWorkingDirectory;
		static readonly PackageActionsToRun packageActionsToRun = new PackageActionsToRun();
		static readonly PackageActionRunner packageActionRunner;
		
		static PackageManagementServices()
		{
			InitializeCoreServices();
			options = new PackageManagementOptions(new Properties());
			registeredPackageRepositories = new RegisteredPackageRepositories(options);
			outputMessagesView = new PackageManagementOutputMessagesView(packageManagementEvents);
			solution = new PackageManagementSolution(registeredPackageRepositories, packageManagementEvents);
			consoleHostProvider = new PackageManagementConsoleHostProvider(solution, registeredPackageRepositories);
			projectBrowserRefresher = new ProjectBrowserRefresher(projectService, packageManagementEvents);
			runPackageInitializationScripts = new RunPackageInitializationScriptsOnSolutionOpen(projectService);
			resetPowerShellWorkingDirectory = new ResetPowerShellWorkingDirectoryOnSolutionClosed(projectService, ConsoleHost);
			var consolePackageActionRunner = new ConsolePackageActionRunner(ConsoleHost, packageActionsToRun);
			packageActionRunner = new PackageActionRunner(consolePackageActionRunner, packageManagementEvents);
		}
		
		static void InitializeCoreServices()
		{
			string applicationName = "ICSharpCode.PackageManagement";
			string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string configDirectory = Path.Combine(appDataFolder, applicationName);
			string assemblyFolder = Path.GetDirectoryName(typeof(PackageManagementServices).Assembly.Location);
			string dataDirectory = Path.Combine(assemblyFolder, @"..\data");
			string addinDirectory = Path.Combine(assemblyFolder, @"..\AddIns");
			
			var startup = new CoreStartup(applicationName);
			startup.ConfigDirectory = configDirectory;
			startup.DataDirectory = dataDirectory;
			startup.StartCoreServices();
			InitializeStringResources();
			startup.AddAddInsFromDirectory(addinDirectory);
			startup.RunInitialization();
			
			ICSharpCode.SharpDevelop.Project.ProjectService.InitializeService();
		}

		static void InitializeStringResources()
		{
			var resourceManager = new ResourceManager("ICSharpCode.PackageManagement.Resources.StringResources", typeof(PackageManagementServices).Assembly);
			ResourceService.RegisterNeutralStrings(resourceManager);
		}
		
		public static PackageManagementOptions Options {
			get { return options; }
		}
		
		public static IPackageManagementSolution Solution {
			get { return solution; }
		}
		
		public static IPackageManagementConsoleHost ConsoleHost {
			get { return consoleHostProvider.ConsoleHost; }
		}
		
		public static IRegisteredPackageRepositories RegisteredPackageRepositories {
			get { return registeredPackageRepositories; }
		}
		
		public static IPackageManagementEvents PackageManagementEvents {
			get { return packageManagementEvents; }
		}
		
		public static IPackageManagementOutputMessagesView OutputMessagesView {
			get { return outputMessagesView; }
		}
		
		public static IPackageManagementProjectService ProjectService {
			get { return projectService; }
		}
		
		public static PackageActionsToRun PackageActionsToRun {
			get { return packageActionsToRun; }
		}
		
		public static IPackageActionRunner PackageActionRunner {
			get { return packageActionRunner; }
		}
	}
}
