using System;
using SpaceshipAI.Compiler.Assembler.Lexer;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    /// <summary>
    /// Processes an integer arithmetic expression used in memory addressings
    /// Example:
    /// lea eax, [ebx * 8 + ecx + 2]
    /// </summary>
    public static class IntegerArithmeticProcessor
    {
        public static IntegerArithmeticExpression Process(AsmToken[] tokenLine, int offset)
        {
            int tokenCount = tokenLine.Length;
            bool closed = false;
            if (tokenCount < offset + 2)
            {
                throw new Exception("Wrong formatted instruction at line" + tokenLine[offset].AsmLine);
            }
            int i = 1;
            var expression = new IntegerArithmeticExpression();
            while ((offset + i) < tokenCount)
            {
                AsmToken token = tokenLine[offset + i];
                if ((i & 1) == 1) //Oddth token. It should be either a register or a number.
                {
                    switch (token.Token)
                    {
                        case AsmTokens.Name:

                            string regName = token.Value;
                            if (!X86Assembler.IsRegister(regName))
                            {
                                throw new NotARegisterException(token);
                            }
                            expression.AddTerm(regName);
                            break;
                        case AsmTokens.Number:
                            expression.AddTerm(token.IntValue);
                            break;
                        default:
                            throw new UnexpectedTokenException(token);
                    }
                }
                else //Eventh token. It is either an operator or the end of the expression.
                {
                    switch (token.Token)
                    {
                        case AsmTokens.Plus:
                            expression.AddOperator(IntegerArithmeticOperatorType.Addition);
                            break;
                        case AsmTokens.Mult:
                            expression.AddOperator(IntegerArithmeticOperatorType.Multiplication);
                            break;
                        case AsmTokens.Sub:
                            expression.AddOperator(IntegerArithmeticOperatorType.Substraction);
                            break;
                        case AsmTokens.RightBracket:
                            closed = true;
                            break;
                        default:
                            throw new UnexpectedTokenException(token);
                    }
                }

                i++;
            }
            if (!closed)
                throw new Exception("Wrong formatted instruction at line " + tokenLine[0].AsmLine);

            return expression;
        }
    }
}