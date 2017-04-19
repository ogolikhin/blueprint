using System.Reflection;

[assembly: AssemblyCompany("Blueprint Software Systems Inc.")]
[assembly: AssemblyProduct("Blueprint")]
[assembly: AssemblyCopyright("©2017 Blueprint Software Systems Inc. All rights reserved")]

// The assembly version is used by the runtime for binding, specifically when using strong names and the global assembly cache
[assembly: AssemblyVersion("8.2.0.0")]
// The assembly informational version is the marketing version that we talk to customers about (i.e. "2011 Feature Pack 1")
[assembly: AssemblyInformationalVersion("A0")]

#if DEBUG
[assembly: AssemblyConfiguration ( "DEBUG" )]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
