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
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Framework.Logging;

namespace ICSharpCode.AspNet
{
	public partial class DnxGlobalOptionsPanel : OptionPanel
	{
		readonly bool originalRestoreDependenciesSetting = AspNetServices.ProjectService.RestoreDependencies;
		
		readonly List<LogLevel> logLevels = new List<LogLevel> {
			LogLevel.Debug,
			LogLevel.Verbose,
			LogLevel.Information,
			LogLevel.Warning,
			LogLevel.Error,
			LogLevel.Critical
		};
		
		public DnxGlobalOptionsPanel()
		{
			SelectedLogLevel = DnxLoggerService.LogLevel;
			RestoreDependencies = originalRestoreDependenciesSetting;
			
			InitializeComponent();
			DataContext = this;
		}
		
		public IEnumerable<LogLevel> LogLevels {
			get { return logLevels; }
		}
		
		public LogLevel SelectedLogLevel { get; set; }
		
		public bool RestoreDependencies { get; set; }
		
		public override bool SaveOptions()
		{
 			if (RestoreDependencies != originalRestoreDependenciesSetting) {
 				AspNetServices.ProjectService.RestoreDependencies = RestoreDependencies;
 			}
			
			DnxLoggerService.LogLevel = SelectedLogLevel;
			return true;
		}
	}
}