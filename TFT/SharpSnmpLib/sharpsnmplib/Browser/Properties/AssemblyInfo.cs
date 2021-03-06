#region Using directives

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("#SNMP MIB Browser")]
[assembly: AssemblyDescription("#SNMP MIB Browser")]
[assembly: AssemblyConfiguration("MIT/X11")]
[assembly: AssemblyCompany("LeXtudio")]
[assembly: AssemblyProduct("#SNMP MIB Browser")]
[assembly: AssemblyCopyright("(C) 2008-2010 Lex Li, Steve Santacroce, and other contributors.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible(false)]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all the values or you can use the default the Revision and 
// Build Numbers by using the '*' as shown below:
[assembly: AssemblyVersion("7.0.011121.02")]
[assembly: AssemblyFileVersion("7.0.011121.02")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
