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
using ICSharpCode.Core;
using OmniSharp.Models;
using OmniSharp.Services;

namespace ICSharpCode.AspNet.Omnisharp.SharpDevelop
{
	public class EventEmitter : IEventEmitter
	{
		public void Emit(string kind, object args)
		{
			LoggingService.Debug(string.Format("EventEmitter: Kind,Args: {0},{1}", kind,args));
			if (kind == EventTypes.ProjectChanged) {
				OnProjectChanged(args);
			} else if (kind == EventTypes.PackageRestoreStarted) {
				OnPackageRestoreStarted(args);
			} else if (kind == EventTypes.PackageRestoreFinished) {
				OnPackageRestoreFinished(args);
			}
		}

		void OnProjectChanged(object args)
		{
			var response = args as ProjectInformationResponse;
			if (response == null)
				return;
			
			AspNetServices.ProjectService.OnProjectChanged(response.AspNet5Project);
		}
		
		void OnPackageRestoreStarted(object args)
		{
			var restoreMessage = args as PackageRestoreMessage;
			if (restoreMessage == null)
				return;
		}

		void OnPackageRestoreFinished(object args)
		{
			var restoreMessage = args as PackageRestoreMessage;
			if (restoreMessage == null)
				return;
		}
	}
}
