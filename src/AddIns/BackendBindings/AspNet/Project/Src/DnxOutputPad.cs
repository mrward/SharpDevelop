﻿// Copyright (c) 2015 AlphaSierraPapa for the SharpDevelop Team
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
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.Core.Presentation;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Workbench;
using Microsoft.Framework.Logging;

namespace ICSharpCode.AspNet
{
	public class DnxOutputPad : ILogger
	{
		static readonly DnxOutputPad instance = new DnxOutputPad();
		static MessageViewCategory view = null;
		
		static DnxOutputPad()
		{
			MessageViewCategory.Create(ref view, "DNXOutput", "DNX Output");
			instance = new DnxOutputPad();
		}
		
		DnxOutputPad()
		{
			
			SD.ProjectService.SolutionOpened += SolutionOpened;
		}

		public static DnxOutputPad Instance {
			get { return instance; }
		}
		
		void SolutionOpened (object sender, EventArgs e)
		{
			view.ClearText();
		}

		public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
		{
			if (!IsEnabled (logLevel))
				return;

			string message = formatter.Invoke(state, exception) + Environment.NewLine;
			view.AppendText(message);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= DnxLoggerService.LogLevel;
		}

		public IDisposable BeginScope(object state)
		{
			throw new NotImplementedException ();
		}
	}
}
