using SpaceshipAI.Compiler;
using SpaceshipAI.Compiler.Assembler.Lexer;
using SpaceshipAI.Compiler.Assembler.Parser;
using SpaceshipAI.Compiler.CSharpGeneration;

namespace SpaceshipAI.VM
{
    public class VMCompiler
    {
        public static IVirtualMachine Compile(string asmSource)
        {
            AsmToken[][] asmTokens = AssemblyLexer.LexAsmCode(asmSource);
            X86Assembly assembly = X86Assembler.GenerateAssembly(asmTokens);
            string source = CSharpGenerator.GenerateSource(assembly);
            return CSharpCompiler.CompileCSharp(source);
        }
    }
}