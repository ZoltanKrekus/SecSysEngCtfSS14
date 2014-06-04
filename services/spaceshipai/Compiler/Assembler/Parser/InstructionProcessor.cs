using SpaceshipAI.Compiler.Assembler.Lexer;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    public class InstructionProcessor
    {
        public static X86Instruction ReadInstruction(AsmToken[] tokenLine)
        {
            int tokenCount = tokenLine.Length;
            if (tokenLine[0].Token != AsmTokens.Name)
            {
                throw new UnexpectedTokenException(tokenLine[0], AsmTokens.Name);
            }
            string instructionName = tokenLine[0].Value;
            int line = tokenLine[0].AsmLine;
            if (tokenCount == 1)
            {
                return new X86Instruction(line, instructionName);
            }
            X86Operand dst;
            X86Operand src = null;
            switch (tokenLine[1].Token)
            {
                case AsmTokens.Name:
                    if (X86Assembler.IsRegister(tokenLine[1].Value))
                    {
                        dst = new RegisterArgument(tokenLine[1].Value);
                    }
                    else
                    {
                        dst = new LabelArgument(tokenLine[1].Value);
                    }
                    break;
                case AsmTokens.Number:
                    dst = new ImmediateArgument(tokenLine[1].IntValue);
                    break;
                case AsmTokens.LeftBracket:
                    IntegerArithmeticExpression expr = IntegerArithmeticProcessor.Process(tokenLine, 1);
                    dst = new MemoryArgument(expr);
                    break;
                default:
                    throw new UnexpectedTokenException(tokenLine[1]);
            }
            bool foundSrc = false;
            for (int i = 2; i < tokenCount; i++)
            {
                if (tokenLine[i].Token == AsmTokens.Comma && i < tokenCount - 1)
                {
                    foundSrc = true;
                    continue;
                }
                switch (tokenLine[i].Token)
                {
                    case AsmTokens.Name:
                        if (X86Assembler.IsRegister(tokenLine[i].Value))
                        {
                            src = new RegisterArgument(tokenLine[i].Value);
                        }
                        else
                        {
                            src = new LabelArgument(tokenLine[i].Value);
                        }
                        goto Finish;
                    case AsmTokens.Number:
                        src = new ImmediateArgument(tokenLine[i].IntValue);
                        goto Finish;
                    case AsmTokens.LeftBracket:
                        IntegerArithmeticExpression expr = IntegerArithmeticProcessor.Process(tokenLine, i);
                        src = new MemoryArgument(expr);
                        goto Finish;
                    default:
                        throw new UnexpectedTokenException(tokenLine[i]);
                }
            }
            Finish:
            if (!foundSrc)
            {
                return new X86Instruction(line, instructionName, dst);
            }
            return new X86Instruction(line, instructionName, dst, src);
        }
    }
}