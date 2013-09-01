﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;

namespace ICSharpCode.TextTemplating
{
	public interface IAddInTree
	{
		IEnumerable<IAddIn> GetAddIns();
		List<IServiceProvider> BuildServiceProviders(string path);
	}
}
