using System.Globalization;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    public class X86Instruction
    {
        private readonly int _line;
        private readonly string _name;
        private readonly int _operandCount;

        public X86Instruction(int line, string name)
        {
            if (!X86Assembler.IsInstruction(name))
                throw new UnknownInstructionException(name, line);
            _line = line;
            _name = name;
            _operandCount = 0;
        }

        public X86Instruction(int line, string name, X86Operand dst)
            : this(line, name)
        {
            Dst = dst;
            Dst.Line = line;
            _operandCount = 1;
        }

        public X86Instruction(int line, string name, X86Operand dst, X86Operand src)
            : this(line, name, dst)
        {
            Src = src;
            Src.Line = line;
            _operandCount = 2;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int OperandCount
        {
            get { return _operandCount; }
        }

        public X86Operand Dst { get; private set; }
        public X86Operand Src { get; private set; }
    }

    public abstract class X86Operand
    {
        public OperandTypes Type { get; protected set; }
        public int Line { get; internal set; }

        public string GetCode(IOperandTranslator translator)
        {
            return translator.Translate(this);
        }
    }

    public sealed class RegisterArgument : X86Operand
    {
        private readonly string _registerName;

        public RegisterArgument(string registerName)
        {
            Type = OperandTypes.Register;
            _registerName = registerName;
        }

        public string Register
        {
            get { return _registerName; }
        }

        public override string ToString()
        {
            return Register;
        }
    }

    public sealed class ImmediateArgument : X86Operand
    {
        private readonly int _value;

        public ImmediateArgument(int value)
        {
            Type = OperandTypes.Immediate;
            _value = value;
        }

        public int Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public sealed class MemoryArgument : X86Operand
    {
        public MemoryArgument(IntegerArithmeticExpression expression)
        {
            Type = OperandTypes.Memory;
            Expression = expression;
        }

        public IntegerArithmeticExpression Expression { get; private set; }
    }

    public sealed class LabelArgument : X86Operand
    {
        private readonly string _label;

        public LabelArgument(string label)
        {
            Type = OperandTypes.Label;
            _label = label;
        }

        public string Label
        {
            get { return _label; }
        }


        public override string ToString()
        {
            return Label;
        }
    }

    public enum OperandTypes
    {
        Register,
        Label,
        Immediate,
        Memory
    }

    public interface IOperandTranslator
    {
        string Translate(X86Operand operand);
    }
}