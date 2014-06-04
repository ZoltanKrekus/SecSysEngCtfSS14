using System;
using System.Text;
using SpaceshipAI.Compiler.Assembler.Parser;

namespace SpaceshipAI.Compiler.CSharpGeneration
{
    public class CSharpTranslator : IOperandTranslator
    {
        private readonly X86Assembly _assembly;
        private readonly bool _yieldAddress;

        public CSharpTranslator(X86Assembly assembly)
        {
            _assembly = assembly;
        }

        public CSharpTranslator(X86Assembly assembly, bool yieldAddress) : this(assembly)
        {
            _yieldAddress = yieldAddress;
        }

        public string Translate(X86Operand operand)
        {
            switch (operand.Type)
            {
                case OperandTypes.Immediate:
                    return TranslateImmediate(((ImmediateArgument) operand));
                case OperandTypes.Label:
                    return TranslateLabel(((LabelArgument) operand));
                case OperandTypes.Register:
                    return TranslateRegister(((RegisterArgument) operand));
                case OperandTypes.Memory:
                    return TranslateMemory(((MemoryArgument) operand));
            }
            //this should never happen
            throw new UnknownInstructionException("-", operand.Line);
        }

        private static string TranslateImmediate(ImmediateArgument arg)
        {
            return "(int) 0x" + arg.Value.ToString("X");
        }

        private string TranslateLabel(LabelArgument arg)
        {
            if (X86Assembler.IsFunction(arg.Label))
                return arg.Label;
            if (!_assembly.NameExists(arg.Label))
            {
                throw new LabelMissingException(arg.Line, arg.Label);
            }
            return _assembly.InternalNameOf(arg.Label);
        }

        private static string GetRegister(string registerName)
        {
            switch (registerName)
            {
                case "al":
                    return "vm.Eax.Lb";
                case "ah":
                    return "vm.Eax.Hb";
                case "ax":
                    return "vm.Eax.W";
                case "eax":
                    return "vm.Eax.Value";
                case "bl":
                    return "vm.Ebx.Lb";
                case "bh":
                    return "vm.Ebx.Hb";
                case "bx":
                    return "vm.Ebx.W";
                case "ebx":
                    return "vm.Ebx.Value";
                case "cl":
                    return "vm.Ecx.Lb";
                case "ch":
                    return "vm.Ecx.Hb";
                case "cx":
                    return "vm.Ecx.W";
                case "ecx":
                    return "vm.Ecx.Value";
                case "dl":
                    return "vm.Edx.Lb";
                case "dh":
                    return "vm.Edx.Hb";
                case "dx":
                    return "vm.Edx.W";
                case "edx":
                    return "vm.Edx.Value";
                case "bp":
                    return "vm.Ebp.W";
                case "ebp":
                    return "vm.Ebp.Value";
                case "sp":
                    return "vm.Esp.W";
                case "esp":
                    return "vm.Esp.Value";
                case "si":
                    return "vm.Esi.W";
                case "esi":
                    return "vm.Esi.Value";
                case "di":
                    return "vm.Edi.W";
                case "edi":
                    return "vm.Edi.Value";
            }
            throw new Exception("Unknown register " + registerName + ". This should never happen!");
        }

        private static string TranslateRegister(RegisterArgument arg)
        {
            return GetRegister(arg.Register);
        }

        private string TranslateMemory(MemoryArgument arg)
        {
            var sb = new StringBuilder();
            foreach (IntegerArithmeticExpressionMember member in arg.Expression.Members)
            {
                if (member is IntegerArithmeticOperator)
                {
                    sb.Append((member as IntegerArithmeticOperator));
                }
                if (member is IntegerArithmeticExpressionTerm)
                {
                    var term = member as IntegerArithmeticExpressionTerm;
                    if (term.Type == IntegerArithmeticExpressionTermType.Register)
                        sb.Append(GetRegister(term.Value));
                    if (term.Type == IntegerArithmeticExpressionTermType.Constant)
                        sb.Append(term.Value);
                }
            }
            if (_yieldAddress)
                return sb.ToString();
            return "vm[" + sb + "]";
        }
    }
}