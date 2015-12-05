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
using System.IO;
using System.Linq;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.AspNet
{
	public static class ISolutionExtensions
	{
		public static bool HasAspNetProjects(this ISolution solution)
		{
			return solution.GetAspNetProjects().Any();
		}
		
		public static IEnumerable<AspNetProject> GetAspNetProjects(this ISolution solution)
		{
			return solution.Projects.OfType<AspNetProject>();
		}
		
		public static AspNetProject FindProjectByProjectJsonFileName(this ISolution solution, string fileName)
		{
			var directory = DirectoryName.Create(Path.GetDirectoryName(fileName));
			return solution.Projects
				.OfType<AspNetProject>()
				.FirstOrDefault(project => project.Directory == directory);
		}
	}
}
