
// <PackageReference Include="System.Runtime.Loader" Version="4.0.0-*" />
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Dotnet
{
    public static class Roslyn
    {
        public const string SampleCodeToCompile = @"
            using System;
            namespace RoslynCompileSample
            {
                public class Writer
                {
                    public void Write(string message)
                    {
                        Console.WriteLine($""you said '{message}!'"");
                    }
                }
            }";

        static Action<string> WriteLine = Console.WriteLine;
        
        public static object Compile(string[] args)
        {
            WriteLine("Let's compile!");
            
            string codeToCompile = args[0];

            WriteLine("Parsing the code into the SyntaxTree");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
            
            string assemblyName = Path.GetRandomFileName();
            var refPaths = new [] {
                typeof(System.Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(
                    Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), 
                    "System.Runtime.dll")
            };

            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            WriteLine("Adding the following references");
            foreach(var r in refPaths) {
                WriteLine(r);
            }

            WriteLine("Compiling ...");
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    WriteLine("Compilation failed!");
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => 
                        diagnostic.IsWarningAsError || 
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    WriteLine("Compilation successful! Now instantiating and executing the code ...");
                    ms.Seek(0, SeekOrigin.Begin);
                    
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    return assembly;
                }

            }
            return null;  // Error
        }

        public static void Invoke(Assembly assembly)
        {
            var type= assembly.GetType("RoslynCompileSample.Writer");
            var instance = assembly.CreateInstance("RoslynCompileSample.Writer");
            var meth = type.GetMember("Write").First() as MethodInfo;
            meth.Invoke(instance, new [] {"joel"});
        }
    }
}
    
    