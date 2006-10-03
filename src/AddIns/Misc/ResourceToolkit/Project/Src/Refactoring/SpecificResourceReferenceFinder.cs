﻿// <file>
//     <copyright see="prj:///Doc/copyright.txt"/>
//     <license see="prj:///Doc/license.txt"/>
//     <owner name="Christian Hornung" email="c-hornung@gmx.de"/>
//     <version>$Revision$</version>
// </file>

using System;

using ICSharpCode.Core;

using Hornung.ResourceToolkit.Resolver;

namespace Hornung.ResourceToolkit.Refactoring
{
	/// <summary>
	/// Finds references to a specific resource in a text document.
	/// </summary>
	public class SpecificResourceReferenceFinder : IResourceReferenceFinder
	{
		readonly string resourceFileName;
		readonly string key;
		
		/// <summary>
		/// Gets the name of the resource file that contains the resource to find.
		/// </summary>
		public string ResourceFileName {
			get {
				return resourceFileName;
			}
		}
		
		/// <summary>
		/// Gets the resource key to find.
		/// </summary>
		public string Key {
			get {
				return key;
			}
		}
		
		// ********************************************************************************************************************************
		
		/// <summary>
		/// Returns the offset of the next possible resource reference in the file
		/// after prevOffset.
		/// Returns -1, if there are no more possible references.
		/// </summary>
		/// <param name="fileName">The name of the file that is currently being searched in.</param>
		/// <param name="fileContent">The text content of the file.</param>
		/// <param name="prevOffset">The offset of the last found reference or -1, if this is the first call in the current file.</param>
		public int GetNextPossibleOffset(string fileName, string fileContent, int prevOffset)
		{
			string code;
			int pos = ResourceRefactoringService.FindStringLiteral(fileName, fileContent, this.Key, prevOffset+1, out code);
			if (pos == -1) {
				// if the code generator search fails, try a direct search
				pos = fileContent.IndexOf(this.Key, prevOffset+1, StringComparison.InvariantCultureIgnoreCase);
			}
			return pos;
		}
		
		/// <summary>
		/// Determines whether the specified ResourceResolveResult describes
		/// a resource that should be included in the search result.
		/// </summary>
		public bool IsReferenceToResource(ResourceResolveResult result)
		{
			return FileUtility.IsEqualFileName(this.ResourceFileName, result.FileName) &&
				result.Key.Equals(this.Key, StringComparison.InvariantCultureIgnoreCase);
		}
		
		// ********************************************************************************************************************************
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SpecificResourceReferenceFinder"/> class.
		/// </summary>
		/// <param name="resourceFileName">The name of the resource file that contains the resource to find.</param>
		/// <param name="key">The resource key to find.</param>
		public SpecificResourceReferenceFinder(string resourceFileName, string key)
		{
			if (resourceFileName == null) {
				throw new ArgumentNullException("resourceFileName");
			}
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			
			this.resourceFileName = resourceFileName;
			this.key = key;
		}
	}
}
