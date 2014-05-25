// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;

namespace ICSharpCode.PackageManagement
{
	/// <summary>
	/// Preprocess the command line so that Pash can process it.
	/// </summary>
	public static class PashCommandLinePreprocessor
	{
		public static string Process(string line)
		{
			return QuoteVersionParameter(line);
		}
		
		static string QuoteVersionParameter(string line)
		{
			int versionOptionIndex = line.IndexOf("-version ", StringComparison.OrdinalIgnoreCase);
			if (versionOptionIndex < 0) {
				return line;
			}
			
			int versionStartIndex = versionOptionIndex + 9;
			int versionEndIndex = line.IndexOf(' ', versionStartIndex);
			if (versionEndIndex < 0) {
				versionEndIndex = line.Length;
			}
			
			int versionLength = versionEndIndex - versionStartIndex;
			if (versionLength <= 0) {
				return line;
			}
			
			string version = line.Substring(versionStartIndex, versionLength);
			
			return line.Substring(0, versionStartIndex) + 
				"\"" + version + "\"" +
				line.Substring(versionEndIndex);
		}
	}
}
