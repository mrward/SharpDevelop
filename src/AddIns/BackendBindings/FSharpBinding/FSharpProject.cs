﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.Exceptions;
using System.IO;

namespace FSharpBinding
{
	//Global todos
	//TODO add "compiling" dialog to output the correct directory
	//TODO copy all config to the correct locations
	//TODO add directory structure
	
	public class FSharpProject : CompilableProject
	{
		public FSharpProject(ProjectLoadInformation info) : base(info)
		{
		}
		
		public FSharpProject(ProjectCreateInformation info) : base(info)
		{
		}
		
		public override string Language {
			get {
				return "F#";
			}
		}
		
		public override LanguageProperties LanguageProperties {
			get {
				return LanguageProperties.None;
			}
		}
		
		protected override ProjectBehavior CreateDefaultBehavior()
		{
			return new FSharpProjectBehavior(this, base.CreateDefaultBehavior());
		}
		
		public void DisableWatcher()
		{
			watcher.Disable();
		}
		
		public void EnableWatcher()
		{
			watcher.Enable();
		}
	}
	
	public class FSharpProjectBehavior : ProjectBehavior
	{
		public FSharpProjectBehavior(FSharpProject project, ProjectBehavior next = null)
			: base(project, next)
		{
			
		}
		
		public override ItemType GetDefaultItemType(string fileName)
		{
			if (string.Equals(".fs", Path.GetExtension(fileName), StringComparison.InvariantCultureIgnoreCase)) {
				return ItemType.Compile;
			} else if (string.Equals(".fsi", Path.GetExtension(fileName), StringComparison.InvariantCultureIgnoreCase)) {
				return ItemType.Compile;
			} else {
				return base.GetDefaultItemType(fileName);
			}
		}
	}
}
