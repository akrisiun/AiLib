AiLib
=====

.net 4.5 Web data light data objects library

### WpfLib

.net 4.5 Presentation framework data objects

WpfExec - sql execute utility

![WpfExec screenshot](https://github.com/akrisiun/AiLib/blob/master/WpfExec.png "WpfExec screenshot")

### Windows 10 UAP - Universal Windows Platform

```
"Microsoft.NETCore.UniversalWindowsPlatform": "5.0.0",
```

### SQLite.UAP.2015

Install-Package SQLite.Net-PCL  
"SQLite.Net-PCL": "3.0.5"

SQLite.UAP.2015 depends on  Microsoft.VCLibs, version=14.0
C:\Program Files (x86)\Microsoft SDKs\UAP\v0.8.0.0\ExtensionSDKs\SQLite.UAP.2015\3.8.11.1\

Severity	Code	Description	Project	File	Line
Warning		The SDK "SQLite.UAP.2015, Version=3.8.11.1" depends on the following SDK(s) "Microsoft.VCLibs, version=14.0", 
which have not been added to the project or were not found. Please ensure that you add these dependencies to your project or you may experience runtime issues. 
You can add dependencies to your project through the Reference Manager.	HelloWorldUap	C:\Program Files (x86)\MSBuild\14.0\bin\Microsoft.Common.CurrentVersion.targets	2048


### Usefull UAP links

http://blogs.msdn.com/b/dotnet/archive/2015/07/30/universal-windows-apps-in-net.aspx?CommentPosted=true#  
https://bitbucket.org/twincoders/sqlite-net-extensions  
http://dailydotnettips.com/2015/08/11/how-to-implement-drag-and-drop-in-windows-universal-apps/  
http://igrali.com/2015/05/01/using-sqlite-in-windows-10-universal-apps/
https://github.com/ljw1004/blog/blob/master/Analyzers/PlatformSpecificAnalyzer/ReadMe.md

https://msdn.microsoft.com/en-us/library/windows/apps/xaml/br230301.aspx  
https://msdn.microsoft.com/en-us/library/windows/apps/windows.foundation.metadata.apiinformation.aspx  
https://msdn.microsoft.com/en-us/library/ms743618(v=vs.100).aspx - Xaml base elements
http://microsoft.github.io/Win2D/html/Introduction.htm

```
if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) 
{ 
    await StatusBar.GetForCurrentView().HideAsync(); 
} 
```

UWP apps use CoreCLR for Debug and .NET Native for Release
Debug build: CoreCLR. When you build your UWP app in Debug mode, it uses the ".NET Core CLR" runtime, the same as used in ASP.NET 5. 
This provides a great edit+run+debug experience – fast deploy, rich debugging, Edit and Continue. It also means

Release build: .NET Native. When you build in Release mode, it takes an additional 30+ seconds to turn 
your MSIL and your references into optimized native machine code. We're working on improving that time. 
It does "tree-shaking" to remove all code that will never be called. It does " Marshalling Code Generation" to 
pre-compile serialization code so it doesn't have to use reflection at runtime. It does whole-program optimization. 
This work and compilation to native code results in a single native DLL. You can explore this in bin\x86\Release\ilc.
