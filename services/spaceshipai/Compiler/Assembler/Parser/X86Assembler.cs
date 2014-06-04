using System;
using System.Collections.Generic;
using System.Text;
using SpaceshipAI.Compiler.Assembler.Lexer;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    public class X86Assembler
    {
        ///////////////////////////////////////////////
        // Static declarations
        ///////////////////////////////////////////////

        private static readonly HashSet<string> Instructions = new HashSet<string>
        {
            "add",
            "and",
            "call",
            "cmp",
            "div",
            "jmp",
            "je",
            "jl",
            "jle",
            "jne",
            "jg",
            "jge",
            "jz",
            "jnz",
            "lea",
            "loop",
            "mov",
            "mul",
            "or",
            "pop",
            "push",
            "ret",
            "shl",
            "shr",
            "sub",
            "xor"
        };

        private static readonly HashSet<string> Registers = new HashSet<string>
        {
            "al",
            "ah",
            "ax",
            "eax",
            "bl",
            "bh",
            "bx",
            "ebx",
            "cl",
            "ch",
            "cx",
            "ecx",
            "dl",
            "dh",
            "dx",
            "edx",
            "bp",
            "ebp",
            "sp",
            "esp",
            "si",
            "esi",
            "di",
            "edi"
        };

        private static readonly HashSet<string> BuiltInFunctions = new HashSet<string>
        {
            "print",
            "printint",
            "procaddr",
            "usercount",
            "memsize"
        };

        ///////////////////////////////////////////////
        // Static declarations end
        ///////////////////////////////////////////////


        ///////////////////////////////////////////////
        // Object declarations
        ///////////////////////////////////////////////
        private readonly AsmToken[][] _asmTokens;
        private X86Assembly _assembly;
        private int _currentLine;

        public X86Assembler(AsmToken[][] asmTokens)
        {
            _asmTokens = asmTokens;
        }

        private AsmToken[] CurrentLine
        {
            get { return _asmTokens[_currentLine]; }
        }

        private int CurrentLineSize
        {
            get { return _asmTokens[_currentLine].Length; }
        }

        private bool Eof
        {
            get { return _currentLine >= _asmTokens.Length; }
        }

        internal static bool IsRegister(string name)
        {
            return Registers.Contains(name);
        }

        internal static bool IsFunction(string name)
        {
            return BuiltInFunctions.Contains(name);
        }

        internal static bool IsInstruction(string text)
        {
            return Instructions.Contains(text);
        }

        public static X86Assembly GenerateAssembly(AsmToken[][] asmTokens)
        {
            var assembler = new X86Assembler(asmTokens);
            return assembler.GenerateAssembly();
        }

        public X86Assembly GenerateAssembly()
        {
            _currentLine = 0;
            _assembly = new X86Assembly();

            do
            {
                if (IsDataSectionComing())
                {
                    NextLine();
                    ProcessDataSection();
                    break;
                }
                if (IsCodeSectionComing())
                {
                    NextLine();
                    ProcessCodeSession();
                    break;
                }
            } while (NextLine());
            if (_assembly.SectionCount == 0)
                _assembly.AddSection(new X86AssemblySection());
            return _assembly;
        }

        private bool NextLine()
        {
            if (_currentLine < _asmTokens.Length - 1)
            {
                _currentLine++;
                return true;
            }
            return false;
        }

        private void MatchTokens(int offset, params AsmTokens[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (CurrentLine[offset + i].Token != tokens[i])
                    throw new UnexpectedTokenException(CurrentLine[offset + i]);
            }
        }

        private void MatchTokens(params AsmTokens[] tokens)
        {
            MatchTokens(0, tokens);
        }

        private void ProcessDataSection()
        {
            if (Eof)
                return;
            do
            {
                if (IsCodeSectionComing())
                {
                    NextLine();
                    ProcessCodeSession();
                    return;
                }
                if (Eof)
                    return;
                if (CurrentLineSize < 4)
                    throw new ArgumentException("Error at line " + CurrentLine[0].AsmLine + ". Wrong data definition.");
                MatchTokens(AsmTokens.Name, AsmTokens.Colon, AsmTokens.Name);
                string constantName = CurrentLine[0].Value;
                if (_assembly.NameExists(constantName))
                {
                    throw new ConstantRedeclaredException(CurrentLine[0]);
                }

                string op = CurrentLine[2].Value;
                if (op == "db")
                {
                    var dBytes = new List<byte>();
                    if ((CurrentLineSize & 1) == 1)
                    {
                        throw new ArgumentException("Error at line " + CurrentLine[0].AsmLine +
                                                    ". Wrong data definition.");
                    }
                    for (int i = 3; i < CurrentLineSize; i++)
                    {
                        if ((i & 1) == 0 && CurrentLine[i].Token != AsmTokens.Comma)
                        {
                            throw new UnexpectedTokenException(CurrentLine[i]);
                        }
                        if ((i & 1) == 1 &&
                            (CurrentLine[i].Token != AsmTokens.Number &&
                             CurrentLine[i].Token != AsmTokens.EncapsedString))
                        {
                            throw new UnexpectedTokenException(CurrentLine[i]);
                        }
                        if (CurrentLine[i].Token == AsmTokens.EncapsedString)
                        {
                            ////////////////////////////////////////////////////////////////
                            // Found a string
                            dBytes.AddRange(Encoding.ASCII.GetBytes(CurrentLine[i].Value));
                            ////////////////////////////////////////////////////////////////
                        }
                        if (CurrentLine[i].Token == AsmTokens.Number)
                        {
                            if (CurrentLine[i].IntValue < 0 || CurrentLine[i].IntValue > 255)
                            {
                                throw new ArgumentException("Number is not a byte at line: " + CurrentLine[i].AsmLine);
                            }
                            ////////////////////////////////////////////////////////////////
                            // Found a byte
                            dBytes.Add((byte) CurrentLine[i].IntValue);
                            ////////////////////////////////////////////////////////////////
                        }
                    }
                    ////////////////////////////////////////////////////////////////
                    // Adding byte array
                    _assembly.AddData(constantName, dBytes.ToArray());
                    ////////////////////////////////////////////////////////////////
                    continue;
                }
                if (op == "equ")
                {
                    if (CurrentLine[3].Token != AsmTokens.Number)
                    {
                        throw new UnexpectedTokenException(CurrentLine[3]);
                    }
                    ////////////////////////////////////////////////////////////////
                    // Adding integer constant
                    _assembly.AddConstant(constantName, CurrentLine[3].IntValue);
                    ////////////////////////////////////////////////////////////////
                }
            } while (NextLine());
        }


        /// <summary>
        /// Requires to passes. First to register the labels, secondly for the actual code generation.
        /// </summary>
        private void ProcessCodeSession()
        {
            if (Eof)
                return;
            var section = new X86AssemblySection();
            do
            {
                if (IsDataSectionComing())
                {
                    NextLine();
                    ProcessDataSection();
                    break;
                }
                if (Eof)
                    return;
                MatchTokens(AsmTokens.Name);

                string name = CurrentLine[0].Value;

                if (IsInstruction(name))
                {
                    X86Instruction instruction = InstructionProcessor.ReadInstruction(CurrentLine);
                    section.AddInstruction(instruction);
                    continue;
                }
                
                if (CurrentLineSize > 1 && CurrentLine[1].Token == AsmTokens.Colon)
                {
                    if (_assembly.NameExists(name))
                    {
                        throw new LabelRedeclaredException(CurrentLine[0]);
                    }
                    if (!(section.DefaultLabel && section.InstructionCount == 0))
                        _assembly.AddSection(section);
                    section = new X86AssemblySection(name);
                    continue;
                }
                throw new UnknownInstructionException(name, CurrentLine[0].AsmLine);
            } while (NextLine());
            _assembly.AddSection(section);
        }

        private bool IsDataSectionComing()
        {
            if (Eof)
                return false;
            return (CurrentLine[0].Token == AsmTokens.Name && CurrentLine[0].Value == "section") && CurrentLineSize == 3 &&
                   CurrentLine[1].Token == AsmTokens.Dot &&
                   (CurrentLine[2].Token == AsmTokens.Name && CurrentLine[2].Value == "data");
        }

        private bool IsCodeSectionComing()
        {
            if (Eof)
                return false;
            return (CurrentLine[0].Token == AsmTokens.Name && CurrentLine[0].Value == "section") && CurrentLineSize == 3 &&
                   CurrentLine[1].Token == AsmTokens.Dot &&
                   (CurrentLine[2].Token == AsmTokens.Name && CurrentLine[2].Value == "text");
        }
    }
}