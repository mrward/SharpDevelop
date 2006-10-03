﻿// <file>
//     <copyright see="prj:///Doc/copyright.txt"/>
//     <license see="prj:///Doc/license.txt"/>
//     <owner name="Christian Hornung" email="c-hornung@gmx.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;
using System.Security.Permissions;

// Information about this assembly is defined by the following
// attributes.
//
// change them to the information which is associated with the assembly
// you compile.

[assembly: AssemblyTitle("Hornung.ResourceToolkit")]
[assembly: AssemblyDescription("Assists in working with resources")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution=true)]
[assembly: FileIOPermission(SecurityAction.RequestMinimum, AllLocalFiles=FileIOPermissionAccess.AllAccess)]
[assembly: UIPermission(SecurityAction.RequestMinimum, Clipboard=UIPermissionClipboard.OwnClipboard, Window=UIPermissionWindow.AllWindows)]
