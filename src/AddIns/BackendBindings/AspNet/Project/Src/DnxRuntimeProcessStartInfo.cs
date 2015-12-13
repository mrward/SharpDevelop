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
using System.Diagnostics;
using System.IO;
using OmniSharp.Models;

namespace ICSharpCode.AspNet
{
	public class DnxRuntimeProcessStartInfo
	{
		readonly DnxRuntime runtime;
		
		public DnxRuntimeProcessStartInfo(string runtimePath)
		{
			runtime = new DnxRuntime(runtimePath);
		}
		
		public ProcessStartInfo GetProcessStartInfo(AspNetProject project)
		{
			return new ProcessStartInfo {
				Arguments = GetArguments(project.GetCurrentCommand()),
				FileName = GetRuntimePath(project),
				WorkingDirectory = project.Directory
			};
		}
		
		string GetRuntimePath(AspNetProject project)
		{
			return Path.Combine(GetDnxRuntimePath(project), "bin", "dnx.exe");
		}
		
		string GetArguments(string command)
		{
			if (runtime.UsesCurrentDirectoryByDefault) {
				return command;
			}
			return String.Format(". {0}", command);
		}

		string GetDnxRuntimePath(AspNetProject project)
		{
			DnxFramework framework = project.DefaultRuntimeFramework;
			if (framework == null) {
				return runtime.Path;
			}
			
			return runtime.GetRuntimePath(framework);
		}
	}
}
