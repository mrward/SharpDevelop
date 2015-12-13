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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages;

namespace ICSharpCode.AspNet
{
	public class AspNetProjectBuilder : IDisposable
	{
		AspNetProject project;
		IProgressMonitor monitor;
		IBuildFeedbackSink feedbackSink;
		ManualResetEventSlim waitEvent = new ManualResetEventSlim();
		bool cancelled;
		CancellationTokenRegistration tokenRegistration;
		DiagnosticsMessage[] messages;

		public AspNetProjectBuilder(AspNetProject project, IProgressMonitor monitor, IBuildFeedbackSink feedbackSink)
		{
			this.project = project;
			this.monitor = monitor;
			this.feedbackSink = feedbackSink;
		}

		public string ProjectPath {
			get { return project.JsonPath; }
		}

		void CancelRequested()
		{
			cancelled = true;
			waitEvent.Set ();
		}

		public void Dispose()
		{
			IProgressMonitor currentMonitor = monitor;
			if (currentMonitor != null) {
				tokenRegistration.Dispose();
				monitor = null;
			}
		}
		
		public Task<bool> Build()
		{
			if (!AspNetServices.ProjectService.HasCurrentDnxRuntime) {
				ReportDnxRuntimeError();
				return Task.FromResult(false);
			}
			
			tokenRegistration = monitor.CancellationToken.Register(CancelRequested);
			var task = Task.Run(() => BuildInternal(), monitor.CancellationToken);
			task.ContinueWith(t => Dispose());
			
			AspNetServices.ProjectService.GetDiagnostics(this);
			
			return task;
		}
		
		public void OnDiagnostics(DiagnosticsMessage[] messages)
		{
			this.messages = messages;
			waitEvent.Set();
		}
		
		bool BuildInternal()
		{
			waitEvent.Wait();
			
			if (cancelled || messages == null) {
				return true;
			}
			
			return ReportBuildResult();
		}
		
		bool ReportBuildResult()
		{
			foreach (DiagnosticsMessage message in messages) {
				if (project.CurrentFramework == message.Framework.FrameworkName) {
					message.ReportBuildResult(feedbackSink);
					return !message.Errors.Any();
				}
			}
			return true;
		}
		
		void ReportDnxRuntimeError ()
		{
			var buildError = new BuildError {
				ErrorText = AspNetServices.ProjectService.CurrentRuntimeError
			};
			feedbackSink.ReportError(buildError);
		}
	}
}
