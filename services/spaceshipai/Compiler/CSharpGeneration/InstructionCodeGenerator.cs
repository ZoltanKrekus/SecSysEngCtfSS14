using System.Diagnostics;
using SpaceshipAI.Compiler.Assembler.Parser;

namespace SpaceshipAI.Compiler.CSharpGeneration
{
    public class InstructionCodeGenerator
    {
        public static void JumpInstruction(ProgramSource ps, X86Assembly assembly, X86Instruction x86Instruction)
        {
            if (x86Instruction.OperandCount != 1)
                throw new WrongParameterException(x86Instruction);
            if (x86Instruction.Dst.Type != OperandTypes.Label || x86Instruction.Dst as LabelArgument == null)
                throw new WrongParameterException(x86Instruction);
            var dst = x86Instruction.Dst as LabelArgument;
            string instructionName = x86Instruction.Name;
            string realLabel = dst.Label;

            if (!assembly.LabelExists(realLabel))
            {
                throw new LabelMissingException(x86Instruction.Line, realLabel);
            }
            string label = assembly.InternalNameOf(realLabel);
            switch (instructionName)
            {
                case "jmp":
                    ps.AddCodeLine("goto ", label);
                    return;
                case "je":
                    ps.AddJump("ZF == 1", label);
                    return;
                case "jne":
                    ps.AddJump("ZF == 0", label);
                    return;
                case "jg":
                    ps.AddJump("ZF == 0 && SF == OF", label);
                    return;
                case "jge":
                    ps.AddJump("SF == OF", label);
                    return;
                case "jl":
                    ps.AddJump("SF != OF", label);
                    return;
                case "jle":
                    ps.AddJump("ZF == 1 || SF != OF", label);
                    return;
                case "jz":
                    ps.AddJump("ZF == 1", label);
                    return;
                case "jnz":
                    ps.AddJump("ZF == 0", label);
                    return;
                case "loop":
                    ps.AddCodeLine("vm.Ecx.Value--");
                    ps.AddJump("vm.Ecx.Value != 0", label);
                    return;
            }
            Debug.Assert(false, "Unknown jump instruction");
        }


        public static void GenerateCmp(ProgramSource ps, string value1, string value2)
        {
            ps.AddLine("{");
            {
                ps.AddLine("ulong temp = (ulong)(" + value1 + ") - (ulong)(" + value2 + ");", 1);
                ps.AddLine("SF = ((0x80000000 & temp) != 0) ? 1 : 0;", 1);
                ps.AddLine("ZF = (temp == 0) ? 1 : 0;", 1);
                ps.AddLine("var signedResult = (long) temp;", 1);
                ps.AddLine("OF = (signedResult > int.MaxValue || signedResult < int.MinValue) ? 1 : 0;", 1);
            }
            ps.AddLine("}");
        }

        public static void Generate(ProgramSource ps, X86Assembly assembly, X86Instruction x86Instruction)
        {
            var translator = new CSharpTranslator(assembly);
            var addressTranslator = new CSharpTranslator(assembly, true);
            switch (x86Instruction.Name)
            {
                case "add":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " += " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "and":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " &= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "call":
                    if (x86Instruction.OperandCount != 1)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    string function = x86Instruction.Dst.GetCode(translator);
                    if (!X86Assembler.IsFunction(function))
                        throw new UnknownFunctionException(x86Instruction);
                    ps.AddCodeLine("vm." + x86Instruction.Dst.GetCode(translator) + "()");
                    break;
                case "cmp":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    GenerateCmp(ps, x86Instruction.Dst.GetCode(translator), x86Instruction.Src.GetCode(translator));
                    break;
                case "jmp":
                case "je":
                case "jne":
                case "jg":
                case "jge":
                case "jl":
                case "jle":
                case "jz":
                case "jnz":
                case "loop":
                    JumpInstruction(ps, assembly, x86Instruction);
                    break;
                case "lea":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                    {
                        ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " = " +
                                       x86Instruction.Src.GetCode(translator));
                        break;
                    }
                    if (x86Instruction.Src.Type == OperandTypes.Memory)
                    {
                        ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " = " +
                                       x86Instruction.Src.GetCode(addressTranslator));
                        break;
                    }
                    throw new WrongParameterException(x86Instruction);
                case "mov":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label &&
                        !assembly.ConstantExists(((LabelArgument) x86Instruction.Src).Label))
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " = " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "mul":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " *= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "or":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " |= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "push":
                    if (x86Instruction.OperandCount != 1)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine("vm.Push(" + x86Instruction.Dst.GetCode(translator) + ")");
                    break;
                case "pop":
                    if (x86Instruction.OperandCount != 1)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " = vm.Pop()");
                    break;
                case "ret":
                    if (x86Instruction.OperandCount != 0)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine("return;");
                    break;
                case "shl":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " <<= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "shr":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " >>= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "sub":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " -= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
                case "xor":
                    if (x86Instruction.OperandCount != 2)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Dst.Type != OperandTypes.Memory &&
                        x86Instruction.Dst.Type != OperandTypes.Register)
                        throw new WrongParameterException(x86Instruction);
                    if (x86Instruction.Src.Type == OperandTypes.Label)
                        throw new WrongParameterException(x86Instruction);
                    ps.AddCodeLine(x86Instruction.Dst.GetCode(translator) + " ^= " +
                                   x86Instruction.Src.GetCode(translator));
                    break;
            }
        }
    }
}