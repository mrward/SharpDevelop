﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using ICSharpCode.SharpDevelop.Editor.CodeCompletion;
using ICSharpCode.XmlEditor;
using NUnit.Framework;

namespace XmlEditor.Tests.Schema
{
	/// <summary>
	/// Element that is a simple content type.
	/// </summary>
	[TestFixture]
	public class SimpleContentWithAttributeSchemaTestFixture : SchemaTestFixtureBase
	{
		XmlCompletionItemCollection attributeCompletionItems;
		
		public override void FixtureInit()
		{
			XmlElementPath path = new XmlElementPath();
			path.AddElement(new QualifiedName("foo", "http://foo.com"));
			
			attributeCompletionItems = SchemaCompletion.GetAttributeCompletion(path);
		}
		
		[Test]
		public void BarAttributeExists()
		{
			Assert.IsTrue(attributeCompletionItems.Contains("bar"),
			              "Attribute bar does not exist.");
		}		
	
		protected override string GetSchema()
		{
			return "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\r\n" +
				"\ttargetNamespace=\"http://foo.com\"\r\n" +
				"\txmlns=\"http://foo.com\">\r\n" +
				"\t<xs:element name=\"foo\">\r\n" +
				"\t\t<xs:complexType>\r\n" +
				"\t\t\t<xs:simpleContent>\r\n" +
				"\t\t\t\t<xs:extension base=\"xs:string\">\r\n" +
				"\t\t\t\t\t<xs:attribute name=\"bar\">\r\n" +
				"\t\t\t\t\t\t<xs:simpleType>\r\n" +
				"\t\t\t\t\t\t\t<xs:restriction base=\"xs:NMTOKEN\">\r\n" +
				"\t\t\t\t\t\t\t\t<xs:enumeration value=\"default\"/>\r\n" +
				"\t\t\t\t\t\t\t\t<xs:enumeration value=\"enable\"/>\r\n" +
				"\t\t\t\t\t\t\t\t<xs:enumeration value=\"disable\"/>\r\n" +
				"\t\t\t\t\t\t\t\t<xs:enumeration value=\"hide\"/>\r\n" +
				"\t\t\t\t\t\t\t\t<xs:enumeration value=\"show\"/>\r\n" +
				"\t\t\t\t\t\t\t</xs:restriction>\r\n" +
				"\t\t\t\t\t\t</xs:simpleType>\r\n" +
				"\t\t\t\t\t</xs:attribute>\r\n" +
				"\t\t\t\t\t<xs:attribute name=\"id\" type=\"xs:string\"/>\r\n" +
				"\t\t\t\t\t<xs:attribute name=\"msg\" type=\"xs:string\"/>\r\n" +
				"\t\t\t\t</xs:extension>\r\n" +
				"\t\t\t</xs:simpleContent>\r\n" +
				"\t\t</xs:complexType>\r\n" +
				"\t</xs:element>\r\n" +
				"</xs:schema>";
		}
	}
}
