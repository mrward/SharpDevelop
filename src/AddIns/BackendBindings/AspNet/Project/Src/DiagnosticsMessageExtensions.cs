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
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;
using System.Text.RegularExpressions;

namespace ICSharpCode.AspNet
{
	public static class DiagnosticsMessageExtensions
	{
		public static void ReportBuildResult(this DiagnosticsMessage message, IBuildFeedbackSink feedbackSink)
		{
			AddErrors(feedbackSink, message.Warnings, isWarning: true);
			AddErrors(feedbackSink, message.Errors);
		}

		static void AddErrors(IBuildFeedbackSink feedbackSink, IEnumerable<string> messages, bool isWarning = false)
		{
			foreach (string message in messages) {
				BuildError error = CreateBuildError(message);
				error.IsWarning = isWarning;
				feedbackSink.ReportError(error);
			}
		}

		/// <summary>
		/// Error format:
		/// 
		/// D:\projects\app\src\app\Program.cs(11,26): DNXCore,Version=v5.0 error CS1061: Message
		/// </summary>
		static BuildError CreateBuildError(string message)
		{
			Match match = Regex.Match(message, @"\b(\w:[/\\].*?)\((\d+),(\d+)\):.*?(CS\d+):(.*)");
			if (match.Success) {
				try {
					return new BuildError {
						FileName =  match.Groups[1].Value,
						Line =  Convert.ToInt32(match.Groups[2].Value),
						Column = Convert.ToInt32(match.Groups[3].Value),
						ErrorCode = match.Groups[4].Value,
						ErrorText = match.Groups[5].Value.Trim()
					};
				} catch (FormatException) {
				} catch (OverflowException) {
					// Ignore.
				}
			}

			return new BuildError {
				ErrorText = message
			};
		}
	}
}
