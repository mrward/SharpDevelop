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
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;

namespace ICSharpCode.AspNet
{
	public class DependencyNode : CustomFolderNode
	{
		readonly DependenciesMessage message;
		readonly DependencyDescription dependency;

		public DependencyNode(DependenciesMessage message, DependencyDescription dependency)
		{
			this.message = message;
			this.dependency = dependency;
			
			Text = GetLabel();
			Tag = new DependencyNodeDescriptor(this);
			
			SetIcon();
			
			if (dependency.Dependencies.Any()) {
				var node = new CustomNode();
				node.AddTo(this);
			}
		}
		
		void SetIcon()
		{
			if (Type == "Package") {
				SetFileIcon("nuget-16.png");
			} else if (Unresolved) {
				SetFileIcon("nuget-warning-16.png");
			} else if (Type == "Project") {
				SetFileIcon("project-dependency-16.png");
			} else {
				SetIcon("Icons.16x16.Reference");
			}
		}
		
		void SetFileIcon(string iconFileName)
		{
			SetIcon(@"file:${AddInPath:ICSharpCode.AspNet}\Icons\" + iconFileName);
		}
		
		public string NodeName {
			get { return dependency.Name; }
		}
		
		public override string CompareString {
			get { return NodeName; }
		}

		public string Version {
			get { return dependency.Version; }
		}

		public string Type {
			get { return dependency.Type; }
		}

		public string Path {
			get { return dependency.Path; }
		}
		
		public string GetLabel()
		{
			return String.Format("{0} ({1})", dependency.Name, dependency.Version);
		}
		
		protected override void Initialize()
		{
			AddNodes();
			base.Initialize();
		}
		
		void AddNodes()
		{
			Nodes.Clear();
			foreach (DependencyNode node in GetDependencies()) {
				node.AddTo(this);
			}
		}

		public bool HasDependencies()
		{
			return dependency.Dependencies.Any();
		}

		public IEnumerable<DependencyNode> GetDependencies()
		{
			foreach (DependencyItem item in dependency.Dependencies) {
				var matchedDependency = message.Dependencies[item.Name];
				if (matchedDependency != null) {
					yield return new DependencyNode(message, matchedDependency);
				}
			}
		}

		public bool Unresolved {
			get { return dependency.Type == "Unresolved"; }
		}
	}
}
