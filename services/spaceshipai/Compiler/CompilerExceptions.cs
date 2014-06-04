using System;
using SpaceshipAI.Compiler.Assembler.Lexer;
using SpaceshipAI.Compiler.Assembler.Parser;

namespace SpaceshipAI.Compiler
{
    [Serializable]
    public class UnexpectedTokenException : ParseErrorException
    {
        public UnexpectedTokenException(AsmToken token)
            : base("Unexpected token " + token.Token + " at line " + token.AsmLine + ".")
        {
            Token = token;
        }

        public UnexpectedTokenException(AsmToken token, AsmTokens expected)
            : base(
                "Unexpected token " + token.Token + " " + token.Value + " at line " + token.AsmLine + ". Expected: " +
                expected)
        {
            Token = token;
        }

        public AsmToken Token { get; private set; }
    }

    [Serializable]
    public class LabelRedeclaredException : ParseErrorException
    {
        public LabelRedeclaredException(AsmToken token)
            : base("Label " + token.Value + " redeclared at line " + token.AsmLine + ".")
        {
            Token = token;
        }

        public AsmToken Token { get; private set; }
    }

    [Serializable]
    public class ConstantRedeclaredException : ParseErrorException
    {
        public ConstantRedeclaredException(AsmToken token)
            : base("Constant " + token.Value + " redeclared at line " + token.AsmLine + ".")
        {
            Token = token;
        }

        public AsmToken Token { get; private set; }
    }

    [Serializable]
    public class NotARegisterException : ParseErrorException
    {
        public NotARegisterException(AsmToken token)
            : base(token.Value + " is not a register at line " + token.AsmLine + ".")
        {
            Token = token;
        }

        public AsmToken Token { get; private set; }
    }

    [Serializable]
    public class WrongParameterException : ParseErrorException
    {
        public WrongParameterException(X86Instruction x86Instruction)
            : base("Wrong parameters for instruction " + x86Instruction.Name + " at line: " + x86Instruction.Line + ".")
        {
            X86Instruction = x86Instruction;
        }

        public X86Instruction X86Instruction { get; private set; }
    }

    [Serializable]
    public class UnknownFunctionException : ParseErrorException
    {
        public UnknownFunctionException(X86Instruction x86Instruction)
            : base("Unknown function" + x86Instruction.Dst + " at line: " + x86Instruction.Line + ".")
        {
            X86Instruction = x86Instruction;
        }

        public X86Instruction X86Instruction { get; private set; }
    }

    [Serializable]
    public class UnknownInstructionException : ParseErrorException
    {
        public UnknownInstructionException(string name, int line)
            : base("Unknown instruction " + name + " at line: " + line + ".")
        {
        }
    }

    [Serializable]
    public class ParameterMissingException : ParseErrorException
    {
        public ParameterMissingException(AsmToken token)
            : base("Missing or too much parameter for instruction " + token.Value + " at line " + token.AsmLine + ".")
        {
            Token = token;
        }

        public AsmToken Token { get; private set; }
    }

    [Serializable]
    public class LabelMissingException : ParseErrorException
    {
        public LabelMissingException(int line, string instructionName)
            : base("Label " + instructionName + " is missing at line " + line + ".")
        {
        }
    }

    [Serializable]
    public class ConstantMissingException : ParseErrorException
    {
        public ConstantMissingException(int line, string instructionName)
            : base("Label " + instructionName + " is missing at line " + line + ".")
        {
        }
    }

    [Serializable]
    public class ParseErrorException : Exception
    {
        public ParseErrorException(string message) : base(message)
        {
            
        }
    }
}