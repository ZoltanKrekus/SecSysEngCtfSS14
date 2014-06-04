using System;
using System.Collections.Generic;
using System.Linq;
using SpaceshipAI.Compiler.Assembler.Parser;

namespace SpaceshipAI.Compiler.CSharpGeneration
{
    public class CSharpGenerator
    {
        private readonly X86Assembly _assembly;

        public CSharpGenerator(X86Assembly assembly)
        {
            _assembly = assembly;
        }

        public static string GenerateSource(X86Assembly assembly)
        {
            var generator = new CSharpGenerator(assembly);
            return generator.GenerateProgramSource().Source;
        }

        private static void GenerateDataEntry(ProgramSource ps, string name, IEnumerable<byte> data)
        {
            ps.StartLine("var " + name + " = vm.AddBytes(");
            string bytesStr = String.Join(", ", data.Select(x => "0x" + x.ToString("X2")));
            ps.Append(bytesStr);
            ps.EndLine(");");
        }

        public ProgramSource GenerateProgramSource()
        {
            var ps = new ProgramSource();
            foreach (KeyValuePair<string, byte[]> data in _assembly.Data)
            {
                GenerateDataEntry(ps, data.Key, data.Value);
            }
            foreach (KeyValuePair<string, int> constant in _assembly.Constants)
            {
                ps.AddCodeLine("var ", constant.Key, " = 0x", constant.Value.ToString("X"));
            }

            ps.AddLine("goto _start;");

            foreach (X86AssemblySection section in _assembly.Sections)
            {
                ps.AddLine(_assembly.InternalNameOf(section.Name) + ":");

                if (section.InstructionCount == 0)
                    ps.AddLine(";");
                else
                    foreach (X86Instruction instruction in section.Instructions)
                    {
                        InstructionCodeGenerator.Generate(ps, _assembly, instruction);
                    }
            }

            return ps;
        }
    }
}