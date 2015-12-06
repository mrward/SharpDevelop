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
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.OptionPanels;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.AspNet
{
	public partial class DnxRuntimeOptionsPanel : ProjectOptionPanel
	{
		GlobalJsonFile globalJsonFile;
		List<string> dnxRuntimeVersions;
		string originalDnxRuntimeVersion;
		string selectedDnxRuntimeVersion;
		
		public DnxRuntimeOptionsPanel()
		{
			InitializeComponent();
			DataContext = this;
		}
		
		protected override void Load(MSBuildBasedProject project, string configuration, string platform)
		{
			dnxRuntimeVersions = GetDnxRuntimeVersions().ToList();
			globalJsonFile = GlobalJsonFile.Read((AspNetProject)project);
			if (globalJsonFile.Exists) {
				originalDnxRuntimeVersion = globalJsonFile.DnxRuntimeVersion;
			}
			 
			if (globalJsonFile == null)
				return;
			
			if (globalJsonFile.Exists) {
				SelectDnxRuntimeVersionInComboBox(globalJsonFile.DnxRuntimeVersion);
			}
			
			RaisePropertyChanged(null);
		}

		IEnumerable<string> GetDnxRuntimeVersions()
		{
			return new string[] {
				"1.0.0-beta4",
				"1.0.0-beta5",
				"1.0.0-beta6",
				"1.0.0-beta7",
				"1.0.0-beta8",
				"1.0.0-rc1-final",
				"1.0.0-rc1-update1"
			};
		}
		
		public IEnumerable<string> DnxRuntimeVersions {
			get { return dnxRuntimeVersions; }
		}
		
		public string SelectedDnxRuntimeVersion {
			get { return selectedDnxRuntimeVersion; }
			set {
				selectedDnxRuntimeVersion = value;
				IsDirty = (originalDnxRuntimeVersion != selectedDnxRuntimeVersion);
			}
		}
		
		protected override bool Save(MSBuildBasedProject project, string configuration, string platform)
		{
			if (globalJsonFile == null)
				return true;
		
			globalJsonFile.DnxRuntimeVersion = SelectedDnxRuntimeVersion;
			if (originalDnxRuntimeVersion != globalJsonFile.DnxRuntimeVersion) {
				globalJsonFile.Save();
				FileUtility.RaiseFileSaved(new FileNameEventArgs(globalJsonFile.Path));
			}
			
			IsDirty = false;
			
			return true;
		}
		
		void SelectDnxRuntimeVersionInComboBox(string version)
		{
			selectedDnxRuntimeVersion = dnxRuntimeVersions
				.FirstOrDefault(dnxRuntimeVersion => dnxRuntimeVersion == version);
		}
	}
}