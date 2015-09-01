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
			
			Text = "Dependencies";
			OpenedImage = "ProjectBrowser.ReferenceFolder.Open";
			ClosedImage = "ProjectBrowser.ReferenceFolder.Closed";
			
			var node = new CustomNode();
			node.AddTo(this);
			
			project.DependenciesChanged += ProjectDependenciesChanged;
		}
		
		public override string CompareString {
			get { return compareString; }
		}
		
		public override void Refresh()
		{
			AddNodes();
			base.Refresh();
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
		}

		void ProjectDependenciesChanged(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
