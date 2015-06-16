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
using System.Text;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Templates
{
	class TemplateFileGenerator
	{
		readonly List<FileDescriptionTemplate> files;
		
		public TemplateFileGenerator(IEnumerable<FileDescriptionTemplate> files, string basePath)
		{
			this.files = files.ToList();
			BasePath = basePath;
			ProjectName = "";
			SolutionName = "";
			UserDefinedProjectName = "";
		}
		
		public string ProjectName { get; set; }
		public string UserDefinedProjectName { get; set; }
		public string SolutionName { get; set; }
		public string BasePath { get; private set; }
		
		public void GenerateFiles()
		{
			//Show prompt if any of the files exist
			var existingFileNames = new StringBuilder();
			foreach (FileDescriptionTemplate file in files) {
				string fileName = Path.Combine(BasePath, StringParser.Parse(file.Name, new StringTagPair("ProjectName", ProjectName)));
				
				if (File.Exists(fileName)) {
					if (existingFileNames.Length > 0)
						existingFileNames.Append(", ");
					existingFileNames.Append(Path.GetFileName(fileName));
				}
			}
			
			bool overwriteFiles = true;
			if (existingFileNames.Length > 0) {
				if (!MessageService.AskQuestion(
					StringParser.Parse("${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion}",
					                   new StringTagPair("fileNames", existingFileNames.ToString())),
					"${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion.InfoName}"))
				{
					overwriteFiles = false;
				}
			}
			
			#region Copy files to target directory
			foreach (FileDescriptionTemplate file in files) {
				string fileName = Path.Combine(BasePath, StringParser.Parse(file.Name, new StringTagPair("ProjectName", ProjectName)));
				if (File.Exists(fileName) && !overwriteFiles) {
					continue;
				}
				
				try {
					if (!Directory.Exists(Path.GetDirectoryName(fileName))) {
						Directory.CreateDirectory(Path.GetDirectoryName(fileName));
					}
					if (!String.IsNullOrEmpty(file.BinaryFileName)) {
						// Binary content
						File.Copy(file.BinaryFileName, fileName, true);
					} else {
						// Textual content
						var sr = new StreamWriter(File.Create(fileName), SD.FileService.DefaultFileEncoding);
						string fileContent = StringParser.Parse(file.Content,
						                                        new StringTagPair("ProjectName", ProjectName),
						                                        new StringTagPair("SolutionName", SolutionName),
						                                        new StringTagPair("FileName", fileName));
						fileContent = StringParser.Parse(fileContent);
						if (SD.EditorControlService.GlobalOptions.IndentationString != "\t") {
							fileContent = fileContent.Replace("\t", SD.EditorControlService.GlobalOptions.IndentationString);
						}
						sr.Write(fileContent);
						sr.Close();
					}
				} catch (Exception ex) {
					MessageService.ShowException(ex, "Exception writing " + fileName);
				}
			}
			#endregion
		}
	}
}
