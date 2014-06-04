using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using SpaceshipAI.VM;

namespace SpaceshipAI.Compiler
{
    public class CSharpCompiler
    {
        public static IVirtualMachine CompileCSharp(string cSharpCode)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            var cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("SpaceshipAI.exe");

            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;

            var sb = new StringBuilder();
            sb.AppendLine("using SpaceshipAI.VM;");
            sb.AppendLine("namespace SpaceshipAI.VM {");
            sb.AppendLine("\tpublic class Wrapper : IVirtualMachine {");
            sb.AppendLine("\t\tpublic void Run(ProgramSession vm) {");
            sb.AppendLine("\t\t\tunchecked {");
            sb.AppendLine("\t\t\tvar SF = 0;");
            sb.AppendLine("\t\t\tvar ZF = 0;");
            sb.AppendLine("\t\t\tvar OF = 0;");
            sb.AppendLine(cSharpCode);
            sb.AppendLine("\t\t}"); //Run
            sb.AppendLine("\t\t}"); //Run
            sb.AppendLine("\t}"); //Class declaration
            sb.AppendLine("}"); //Namespace

#if DEBUG 
            Console.WriteLine(sb.ToString());
#endif

            CompilerResults cr = provider.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.Count> 0)
            {
                for (var i = 0; i < cr.Errors.Count; i++)
                {
                    if (cr.Errors[i].IsWarning)
                        continue;
                    Console.WriteLine("{0}", "ERROR: " + cr.Errors[i].ErrorText);
                    throw new Exception("An unknown error occured while compiling the program. Please review your code!");
                }
            }
            Assembly a = cr.CompiledAssembly;
            var o = (IVirtualMachine)a.CreateInstance("SpaceshipAI.VM.Wrapper");
            return o;
        }
    }
}