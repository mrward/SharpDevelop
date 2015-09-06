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
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.Construction;

namespace ICSharpCode.AspNet
{
	public class DefaultAspNetMSBuildProject : MSBuildBasedProject
	{
		public DefaultAspNetMSBuildProject(ProjectCreateInformation information)
			: base(information)
		{
		}

		public override void Save(string fileName)
		{
			lock (SyncRoot) {
				GenerateMSBuildFile();
			}
			base.Save(fileName);
		}

		void GenerateMSBuildFile()
		{
			ToolsVersion = "14.0";
			
			string projectGuid = GetProjectGuid(IdGuid);
			string rootNamespace = RootNamespace;
			
			MSBuildProjectFile.RemoveAllChildren();
			AddVisualStudioProperties();
			AddDnxProps();
			AddGlobalsProperties(projectGuid, rootNamespace);
			AddSchemaVersion();
			AddDnxTargets();
		}

		void AddVisualStudioProperties()
		{
			var propertyGroup = AddPropertyGroup();
			AddPropertyWithNotEmptyCondition(propertyGroup, "VisualStudioVersion", "14.0");
			AddPropertyWithNotEmptyCondition(propertyGroup, "VSToolsPath", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)");
		}

		ProjectPropertyGroupElement AddPropertyGroup()
		{
			var propertyGroup = MSBuildProjectFile.CreatePropertyGroupElement();
			MSBuildProjectFile.AppendChild(propertyGroup);
			return propertyGroup;
		}

		void AddPropertyWithNotEmptyCondition(ProjectPropertyGroupElement propertyGroup, string name, string unevaluatedValue)
		{
			string condition = String.Format("'$({0})' == ''", name);
			propertyGroup.AddProperty(name, unevaluatedValue).Condition = condition;
		}

		void AddDnxProps()
		{
			AddImport(@"$(VSToolsPath)\DNX\Microsoft.DNX.Props", "'$(VSToolsPath)' != ''");
		}

		void AddSchemaVersion()
		{
			AddPropertyGroup().AddProperty("SchemaVersion", "2.0");
		}

		void AddDnxTargets()
		{
			AddImport(@"$(VSToolsPath)\DNX\Microsoft.DNX.targets", "'$(VSToolsPath)' != ''");
		}

		void AddGlobalsProperties(string projectGuid, string rootNamespace)
		{
			var propertyGroup = MSBuildProjectFile.CreatePropertyGroupElement();
			propertyGroup.Label = "Globals";
			MSBuildProjectFile.AppendChild(propertyGroup);
			
			propertyGroup.AddProperty("ProjectGuid",projectGuid);
			propertyGroup.AddProperty("RootNamespace", rootNamespace);
			AddPropertyWithNotEmptyCondition(propertyGroup, "BaseIntermediateOutputPath", @"..\..\artifacts\obj\$(MSBuildProjectName)");
			AddPropertyWithNotEmptyCondition(propertyGroup, "OutputPath", @"..\..\artifacts\bin\$(MSBuildProjectName)\");
		}
		
		static string GetProjectGuid(Guid projectGuid)
		{
			string guid = projectGuid.ToString("B").ToLowerInvariant();
			return guid.Substring(1, guid.Length - 2);
		}
	}
}
