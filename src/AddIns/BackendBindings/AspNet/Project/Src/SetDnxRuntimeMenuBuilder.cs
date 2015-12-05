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
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using OmniSharp.Models;

namespace ICSharpCode.AspNet
{
	public class SetDnxRuntimeMenuBuilder : IMenuItemBuilder
	{
		public IEnumerable<object> BuildItems(Codon codon, object parameter)
		{
			AspNetProject project = AspNetServices.ProjectService.GetStartupDnxProject();
			if (project == null)
				return Enumerable.Empty<object>();
			
			return BuildItems(project);
		}
		
		IEnumerable<object>BuildItems(AspNetProject project)
		{
			yield return CreateMenu(project, null);
			
			foreach (DnxFramework framework in project.GetFrameworks()) {
				yield return CreateMenu(project, framework);
			}
		}
		
		MenuItem CreateMenu(AspNetProject project, DnxFramework framework)
		{
			var menuItem = new MenuItem {
				Header = GetFrameworkName(framework),
				IsChecked = project.DefaultFramework == framework,
				Tag = new ProjectFrameworkInfo(project, framework)
			};
			
			menuItem.Click += MenuItemClick;
			
			return menuItem;
		}
		
		static string GetFrameworkName(DnxFramework framework)
		{
			if (framework == null)
				return "Default";
			
			return framework.FriendlyName;
		}
		
		static void MenuItemClick(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;
			var projectInfo = (ProjectFrameworkInfo)menuItem.Tag;
			
			ISolution solution = SD.ProjectService.CurrentSolution;
			if (solution == null)
				return;
			
			foreach (AspNetProject project in solution.GetAspNetProjects()) {
				project.DefaultFramework = projectInfo.Framework;
			}
		}
	}
}
