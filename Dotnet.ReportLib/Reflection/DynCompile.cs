using Dotnet.Entity;
#if !NETSTANDARD20
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#endif

namespace Dotnet.Reflection
{
    // http://stackoverflow.com/questions/10914484/use-dlr-to-run-code-generated-with-compileassemblyfromsource

    public interface ICalc : ILastError
    {
        // public Exception LastError { get; set; }
        object Calc();
    }

#if !NETSTANDARD20

    public class DynCompile : ILastError
    {
        public class CalcEmpty : ICalc
        {
            public Exception LastError { get; set; }
            public object Calc() { return null; }
        }

        public static ICalc GetCalc(string csCode)
        {
            ICalc obj = null;

#if !NETCOREAPP3_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1
            using (Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                var prm = new System.CodeDom.Compiler.CompilerParameters();
                prm.GenerateInMemory = true;
                prm.GenerateExecutable = false;
#if NET451 || NET471
                prm.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
#endif

                counter++;
                // Implement the interface in the dynamic code
                var res = csProvider.CompileAssemblyFromSource(prm,
                        String.Format(@"public class CompiledCalc{0} : ICalc { public Exception LastError { get; set; }
                            public object Calc() { {1} }}", counter, csCode));
                var type = res.CompiledAssembly.GetType(string.Format("CompiledCalc{0}", counter));

                try
                {
                    obj = Activator.CreateInstance(type) as ICalc;
                }
                catch (Exception ex)
                {
                    obj = obj ?? new CalcEmpty();
                    obj.LastError = ex;
                }
            }
#endif
            return obj;
        }

        static int counter = 0;
        public Exception LastError { get; set; }
        public Assembly Assembly { get; protected set; }
        public string ClassName { get; protected set; }

        public static string DynClass { get { return string.Format("DynClass{0}", counter); } }
        public static string GenCalc(string code)
        {
            var sourceCode =
            @"class DynClass{0} {
                public static object Calc() {
                  {1}
              } }";
            counter++;
            return string.Format(sourceCode, counter, code);
        }

        public static DynCompile CompileCSCode(string csCode, string className, string[] refAssemblies = null)
        {
            if (refAssemblies == null)
                refAssemblies = new string[] { "System.dll" };

            var dyn = new DynCompile() { ClassName = className };

#if !NETCOREAPP3_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1
            CodeDomProvider csharpCodeProvider = new CSharpCodeProvider();
            var cp = new CompilerParameters();
            foreach (string asmName in refAssemblies)
                cp.ReferencedAssemblies.Add(asmName);

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            try
            {
                CompilerResults cr = csharpCodeProvider.CompileAssemblyFromSource(cp, csCode);

                dyn.Assembly = cr.CompiledAssembly;
            }
            catch (Exception ex) { dyn.LastError = ex; }
#endif
            return dyn;
        }

        public object Calculate(string method = "Calc")
        {
            var type = Assembly.GetType(ClassName);
            object result = type.InvokeMember(method, BindingFlags.InvokeMethod, null, Assembly, args: null);
            return result;
        }

        public static void Test() {
            var dyn = DynCompile.CompileCSCode(
                      DynCompile.GenCalc(@"return ""Hello cs world"";"), DynCompile.DynClass, null);
            Console.WriteLine(dyn.Calculate("Calc"));
            var dyn2 = DynCompile.GetCalc(@"return ""Hello Calc object"";");
        }
    }
#endif
        }