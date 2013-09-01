﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NUnit.Framework;
using ICSharpCode.WpfDesign.Designer;
using ICSharpCode.WpfDesign.Designer.Xaml;
using ICSharpCode.WpfDesign.Designer.Services;

namespace ICSharpCode.WpfDesign.Tests.Designer
{
	[TestFixture]
	public class ModelTests : ModelTestHelper
	{
		[Test]
		public void SetButtonWidth()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			button.Properties["Width"].SetValue(100.0);
			AssertCanvasDesignerOutput(@"<Button Width=""100"" />", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void SetButtonWidthElementSyntax()
		{
			DesignItem button = CreateCanvasContext("<Button><Button.Width>50</Button.Width></Button>");
			button.Properties["Width"].SetValue(100.0);
			AssertCanvasDesignerOutput("<Button Width=\"100\">\n</Button>", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void UndoRedoTest()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			UndoService s = button.Context.Services.GetService<UndoService>();
			Assert.IsFalse(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			button.Properties["Width"].SetValue(100.0);
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Width=""100"" />", button.Context);
			s.Undo();
			Assert.IsFalse(s.CanUndo);
			Assert.IsTrue(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button />", button.Context);
			s.Redo();
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Width=""100"" />", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void ButtonClickEventHandler()
		{
			DesignItem button = CreateCanvasContext("<Button Click='OnClick'/>");
			Assert.AreEqual("OnClick", button.Properties["Click"].ValueOnInstance);
			
			button.Properties["Click"].Reset();
			button.Properties["KeyDown"].SetValue("ButtonKeyDown");
			
			AssertCanvasDesignerOutput(@"<Button KeyDown=""ButtonKeyDown"" />", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void ButtonClickEventHandlerUndoRedo()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			UndoService s = button.Context.Services.GetService<UndoService>();
			Assert.IsFalse(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			button.Properties["Click"].SetValue("OnClick");
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Click=""OnClick"" />", button.Context);
			
			button.Properties["Click"].SetValue("OnClick2");
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Click=""OnClick2"" />", button.Context);
			
			s.Undo();
			Assert.IsTrue(s.CanUndo);
			Assert.IsTrue(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Click=""OnClick"" />", button.Context);
			
			s.Undo();
			Assert.IsFalse(s.CanUndo);
			Assert.IsTrue(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button />", button.Context);
			
			s.Redo();
			Assert.IsTrue(s.CanUndo);
			Assert.IsTrue(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Click=""OnClick"" />", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void UndoRedoChangeGroupTest()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			UndoService s = button.Context.Services.GetService<UndoService>();
			Assert.IsFalse(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			using (ChangeGroup g = button.OpenGroup("Resize")) {
				button.Properties["Width"].SetValue(100.0);
				button.Properties["Height"].SetValue(200.0);
				g.Commit();
			}
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Width=""100"" Height=""200"" />", button.Context);
			s.Undo();
			Assert.IsFalse(s.CanUndo);
			Assert.IsTrue(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button />", button.Context);
			s.Redo();
			Assert.IsTrue(s.CanUndo);
			Assert.IsFalse(s.CanRedo);
			AssertCanvasDesignerOutput(@"<Button Width=""100"" Height=""200"" />", button.Context);
			AssertLog("");
		}
		
		
		[Test]
		public void AddTextBoxToCanvas()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Button />\n<TextBox />", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddTextBoxToCanvasExplicitChildrenCollection()
		{
			DesignItem button = CreateCanvasContext("<Canvas.Children><Button/></Canvas.Children>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Canvas.Children>\n  <Button />\n  <TextBox />\n</Canvas.Children>", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddTextBoxToCanvasEmptyImplicitPanelChildrenCollection()
		{
			DesignItem canvas = CreateCanvasContext("<Canvas></Canvas>");
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Canvas>\n" +
			                           "  <TextBox />\n" +
			                           "</Canvas>", canvas.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddTextBoxToCanvasEmptyImplicitPanelChildrenCollectionEmptyTag()
		{
			DesignItem canvas = CreateCanvasContext("<Canvas/>");
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Canvas>\n" +
			                           "  <TextBox />\n" +
			                           "</Canvas>", canvas.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddTextBoxToCanvasEmptyExplicitPanelChildrenCollection()
		{
			DesignItem canvas = CreateCanvasContext("<Canvas><Panel.Children></Panel.Children></Canvas>");
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Canvas>\n" +
			                           "  <Panel.Children>\n" +
			                           "    <TextBox />\n" +
			                           "  </Panel.Children>\n" +
			                           "</Canvas>", canvas.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddTextBoxToCanvasEmptyExplicitCanvasChildrenCollection()
		{
			DesignItem canvas = CreateCanvasContext("<Canvas><Canvas.Children></Canvas.Children></Canvas>");
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			AssertCanvasDesignerOutput("<Canvas>\n" +
			                           "  <Canvas.Children>\n" +
			                           "    <TextBox />\n" +
			                           "  </Canvas.Children>\n" +
			                           "</Canvas>", canvas.Context);
			AssertLog("");
		}
		
		[Test]
		public void InsertTextBoxInCanvas()
		{
			DesignItem button = CreateCanvasContext("<Canvas.Width>50</Canvas.Width><Button/><Canvas.Height>60</Canvas.Height>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			
			DesignItem checkBox = canvas.Services.Component.RegisterComponentForDesigner(new CheckBox());
			canvas.Properties["Children"].CollectionElements.Insert(0, checkBox);
			
			AssertCanvasDesignerOutput("<Canvas.Width>50</Canvas.Width>\n" +
			                           "<CheckBox />\n" +
			                           "<Button />\n" +
			                           "<TextBox />\n" +
			                           "<Canvas.Height>60</Canvas.Height>", button.Context);
			AssertLog("");
		}
		
		[Test]
		public void ClearImplicitChildCollection()
		{
			DesignItem canvas = CreateCanvasContext("<Canvas><Button/><TextBox/></Canvas>");
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Clear();
			AssertCanvasDesignerOutput("<Canvas>\n" +
			                           "</Canvas>", canvas.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddMultiBindingToTextBox()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			
			textBox.Properties[TextBox.TextProperty].SetValue(new MultiBinding());
			DesignItem multiBindingItem = textBox.Properties[TextBox.TextProperty].Value;
			multiBindingItem.Properties["Converter"].SetValue(new MyMultiConverter());

			DesignItemProperty bindingsProp = multiBindingItem.ContentProperty;
			Assert.IsTrue(bindingsProp.IsCollection);
			Assert.AreEqual(bindingsProp.Name, "Bindings");
			
			DesignItem bindingItem = canvas.Services.Component.RegisterComponentForDesigner(new Binding());
			bindingItem.Properties["Path"].SetValue("SomeProperty");
			bindingsProp.CollectionElements.Add(bindingItem);
			
			string expectedXaml = "<Button />\n" +
			                      "<TextBox>\n" +
			                      "  <MultiBinding>\n" +
			                      "    <MultiBinding.Converter>\n" +
			                      "      <t:MyMultiConverter />\n" +
			                      "    </MultiBinding.Converter>\n" +
			                      "    <Binding Path=\"SomeProperty\" />\n" +
			                      "  </MultiBinding>\n" +
			                      "</TextBox>";
			
			AssertCanvasDesignerOutput(expectedXaml, button.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddSimpleBinding()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			
			textBox.Properties[TextBox.TextProperty].SetValue(new Binding());
			textBox.Properties[TextBox.TextProperty].Value.Properties["Path"].SetValue("SomeProperty");
			
			string expectedXaml = "<Button />\n" +
								  "<TextBox Text=\"{Binding Path=SomeProperty}\" />\n";
			
			AssertCanvasDesignerOutput(expectedXaml, button.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddBindingWithStaticResource()
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			
			DesignItemProperty resProp = canvas.Properties.GetProperty("Resources");
			Assert.IsTrue(resProp.IsCollection);
			DesignItem exampleClassItem = canvas.Services.Component.RegisterComponentForDesigner(new ExampleClass());
			exampleClassItem.Key = "bindingSource";
			resProp.CollectionElements.Add(exampleClassItem);
			
			DesignItem bindingItem = canvas.Services.Component.RegisterComponentForDesigner(new Binding());
			textBox.Properties[TextBox.TextProperty].SetValue(bindingItem);
			bindingItem.Properties["Path"].SetValue("StringProp");
			bindingItem.Properties["Source"].SetValue(new StaticResourceExtension());
			bindingItem.Properties["Source"].Value.Properties["ResourceKey"].SetValue("bindingSource");
			
			string expectedXaml = "<Canvas.Resources>\n" +
								  "  <t:ExampleClass x:Key=\"bindingSource\" />\n" +
								  "</Canvas.Resources>\n" +
								  "<Button />\n" +
								  "<TextBox Text=\"{Binding Path=StringProp, Source={StaticResource ResourceKey=bindingSource}}\" />";
			
			AssertCanvasDesignerOutput(expectedXaml, button.Context);
			AssertLog("");
		}
		
		void AddBindingWithStaticResourceWhereResourceOnSameElement(bool setBindingPropertiesAfterSet)
		{
			DesignItem button = CreateCanvasContext("<Button/>");
			DesignItem canvas = button.Parent;
			DesignItem textBox = canvas.Services.Component.RegisterComponentForDesigner(new TextBox());
			canvas.Properties["Children"].CollectionElements.Add(textBox);
			
			DesignItemProperty resProp = textBox.Properties.GetProperty("Resources");
			Assert.IsTrue(resProp.IsCollection);
			DesignItem exampleClassItem = canvas.Services.Component.RegisterComponentForDesigner(new ExampleClass());
			exampleClassItem.Key = "bindingSource";
			resProp.CollectionElements.Add(exampleClassItem);
			
			DesignItem bindingItem = canvas.Services.Component.RegisterComponentForDesigner(new Binding());
			if (!setBindingPropertiesAfterSet) {
				bindingItem.Properties["Path"].SetValue("StringProp");
				bindingItem.Properties["Source"].SetValue(new StaticResourceExtension());
				bindingItem.Properties["Source"].Value.Properties["ResourceKey"].SetValue("bindingSource");
			}
			textBox.Properties[TextBox.TextProperty].SetValue(bindingItem);
			if (setBindingPropertiesAfterSet) {
				bindingItem.Properties["Path"].SetValue("StringProp");
				bindingItem.Properties["Source"].SetValue(new StaticResourceExtension());
				bindingItem.Properties["Source"].Value.Properties["ResourceKey"].SetValue("bindingSource");
			}
			
			string expectedXaml = "<Button />\n" +
								  "<TextBox>\n" +
								  "  <TextBox.Resources>\n" +
								  "    <t:ExampleClass x:Key=\"bindingSource\" />\n" +
								  "  </TextBox.Resources>\n" +
								  "  <Binding Path=\"StringProp\" Source=\"{StaticResource ResourceKey=bindingSource}\" />\n" +
								  "</TextBox>";
			
			AssertCanvasDesignerOutput(expectedXaml, button.Context);
			AssertLog("");
		}
		
		[Test]
		public void AddBindingWithStaticResourceWhereResourceOnSameElement()
		{
			AddBindingWithStaticResourceWhereResourceOnSameElement(false);
		}
		
		[Test]
		public void AddBindingWithStaticResourceWhereResourceOnSameElementAlt()
		{
			AddBindingWithStaticResourceWhereResourceOnSameElement(true);
		}
	}
	
	public class MyMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
		
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return new object[targetTypes.Length];
		}
	}
	
	public class ExampleClass
	{
		string stringProp;
		
		public string StringProp {
			get { return stringProp; }
			set { stringProp = value; }
		}
	}
}
