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
using ICSharpCode.Core;
using ICSharpCode.PackageManagement;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.AspNet
{
	public class DependenciesFolderNode : CustomFolderNode
	{
		readonly AspNetProject project;
		
		static readonly string compareString = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ProjectBrowser.ReferencesNodeText}") + "2";
		
		public DependenciesFolderNode(IProject project)
			: this((AspNetProject)project)
		{
		}
		
		public DependenciesFolderNode(AspNetProject project)
		{
			this.project = project;
			
			ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/DependenciesFolderNode";
			
			Text = "Dependencies";
			SetIcons();
			
			if (AspNetServices.ProjectService.HasCurrentDnxRuntime)
				AddDummyNode();
			
			project.DependenciesChanged += ProjectDependenciesChanged;
			project.PackageRestoreStarted += PackageRestoreStarted;
			project.PackageRestoreFinished += PackageRestoreFinished;
		}

		void SetIcons()
		{
			OpenedImage = "ProjectBrowser.ReferenceFolder.Open";
			ClosedImage = "ProjectBrowser.ReferenceFolder.Closed";
			
			if (!AspNetServices.ProjectService.HasCurrentDnxRuntime) {
				SetIcon(@"file:${AddInPath:ICSharpCode.AspNet}\Icons\ReferenceFolder.Warning.Closed.png");
				ToolTipText = AspNetServices.ProjectService.CurrentRuntimeError;
				OpenedImage = null;
				ClosedImage = null;
			}
		}
		
		void AddDummyNode()
		{
			var node = new CustomNode();
			node.AddTo(this);
		}

		public override string CompareString {
			get { return compareString; }
		}
		
		protected override void Initialize()
		{
			AddNodes();
			base.Initialize();
		}
		
		void AddNodes()
		{
			Nodes.Clear();
			foreach (FrameworkNode node in GetFrameworkFolderNodes()) {
				node.AddTo(this);
			}
		}
		
		public bool HasDependencies ()
		{
			return project.HasDependencies();
		}

		public IEnumerable<FrameworkNode> GetFrameworkFolderNodes ()
		{
			foreach (var dependency in project.GetDependencies()) {
				yield return new FrameworkNode(dependency);
			}
		}
		
		public override void Dispose ()
		{
			project.DependenciesChanged -= ProjectDependenciesChanged;
			project.PackageRestoreStarted -= PackageRestoreStarted;
			project.PackageRestoreFinished -= PackageRestoreFinished;
		}

		void ProjectDependenciesChanged(object sender, EventArgs e)
		{
			Collapse();
			Nodes.Clear();
			AddDummyNode();
			
			isInitialized = false;
		}

		void PackageRestoreStarted(object sender, EventArgs e)
		{
			Text = "Dependencies (Restoring...)";
		}
		
		void PackageRestoreFinished(object sender, EventArgs e)
		{
			Text = "Dependencies";
		}
		
		public override void ActivateItem()
		{
			var command = new ManagePackagesCommand();
			command.Run();
		}
	}
}
