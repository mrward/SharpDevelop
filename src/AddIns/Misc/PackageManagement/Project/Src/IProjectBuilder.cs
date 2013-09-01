﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.PackageManagement
{
	public interface IProjectBuilder
	{
		BuildResults BuildResults { get; }
		
		/// <summary>
		/// Builds the project and waits for the build to complete.
		/// </summary>
		void Build(IProject project);
	}
}
