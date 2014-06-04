using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpaceshipAI.Compiler.Assembler.Lexer
{
    public class AssemblyLexer : AbstractLexer<AsmToken>
    {
        public AssemblyLexer(string input) : base(input)
        {
            Consume();
        }

        private bool IsLetter()
        {
            return char.IsLetter(C) || C == '_';
        }

        private bool IsNumber()
        {
            return char.IsNumber(C) || C == '-';
        }

        private void ConsumeComment()
        {
            while (C != '\n' && C != '\r' && !Eof)
            {
                Consume();
            }
        }

        private void ProcessWhiteSpaces()
        {
            while (C != '\n' && char.IsWhiteSpace(C) && !Eof)
            {
                Consume();
            }
        }


        private AsmToken EncapsedString()
        {
            char separator = C;
            int startLine = CurrentLine;
            int startColumn = CurrentCol;
            bool escape = false;
            string value = "";
            do
            {
                Consume();
                if (C == separator && !escape)
                {
                    Consume();
                    return new AsmToken(CurrentLine, AsmTokens.EncapsedString, value);
                }

                if (C == '\\' && !escape)
                {
                    escape = true;
                    continue;
                }
                escape = false;
                value += C;
            } while (!Eof);
            ThrowError("Unterminated string", startLine, startColumn);
            return null;
        }

        private AsmToken ProcessName()
        {
            var sb = new StringBuilder();
            while ((char.IsLetterOrDigit(C) || C == '_') && !Eof)
            {
                sb.Append(C);
                Consume();
            }
            string result = sb.ToString().ToLowerInvariant();
            return new AsmToken(CurrentLine, AsmTokens.Name, result);
        }

        private static bool IsHexaDigit(char c)
        {
            return (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        private bool IsHexadecimal()
        {
            return char.IsNumber(C) || IsHexaDigit(C);
        }

        private AsmToken ProcessNumber()
        {
            var sb = new StringBuilder();
            bool isHexa = false;
            bool minus = (C == '-');
            if (minus)
                Consume();

            while (IsHexadecimal() && !Eof)
            {
                if (IsHexaDigit(C))
                    isHexa = true;
                sb.Append(C);
                Consume();
            }
            string num = sb.ToString();
            if (string.IsNullOrEmpty(num))
            {
                ThrowError("Unexpected minus sign.");
            }

            bool terminatingH = (C == 'h') || (C == 'H');
            if (isHexa && !terminatingH)
                ThrowError("Hexadecimal number without terminating h.");
            if (terminatingH)
            {
                Consume();
                return new AsmToken(CurrentLine, AsmTokens.Number,
                    (minus ? -1 : 1) + (int) ulong.Parse(num, NumberStyles.HexNumber));
            }
            return new AsmToken(CurrentLine, AsmTokens.Number, (minus ? -1 : 1)*(int) ulong.Parse(num));
        }

        public override IEnumerable<AsmToken> GetTokens()
        {
            Reset();
            while (!Eof)
            {
                switch (C)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                        ProcessWhiteSpaces();
                        continue;
                    case '\n':
                        OnNewLine();
                        Consume();
                        break;
                    case ',':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Comma, ",");
                        break;
                    case '[':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.LeftBracket, "[");
                        break;
                    case ']':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.RightBracket, "]");
                        break;
                    case '.':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Dot, '.');
                        break;
                    case '+':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Plus, '+');
                        break;
                    case '*':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Mult, '*');
                        break;
                    case '-':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Sub, '-');
                        break;
                    case ':':
                        Consume();
                        yield return new AsmToken(CurrentLine, AsmTokens.Colon, ':');
                        break;
                    case ';':
                        ConsumeComment();
                        continue;
                    case '"':
                    case '\'':
                        yield return EncapsedString();
                        break;
                    default:
                        if (IsLetter())
                        {
                            yield return ProcessName();
                            break;
                        }
                        if (IsNumber())
                        {
                            yield return ProcessNumber();
                            break;
                        }
                        ThrowError(string.Format("Unknown token '{0}'", C));
                        break;
                }
            }
        }

        public static AsmToken[][] LexAsmCode(string code)
        {
            var lexer = new AssemblyLexer(code);
            var asmTokens = new LinkedList<AsmToken[]>();
            var currentLine = new List<AsmToken>();
            lexer.NewLine += (s, e) =>
            {
                if (currentLine.Count > 0)
                {
                    asmTokens.AddLast(currentLine.ToArray());
                    currentLine.Clear();
                }
            };

            foreach (AsmToken asmToken in lexer.GetTokens())
            {
                currentLine.Add(asmToken);
            }
            if (currentLine.Count > 0)
                asmTokens.AddLast(currentLine.ToArray());
            return asmTokens.ToArray();
        }
    }

    public enum AsmTokens
    {
        Name,
        Number,
        EncapsedString,
        Dot,
        LeftBracket,
        RightBracket,
        Colon,
        Comma,
        Plus,
        Mult,
        Sub
    }

    public class AsmToken
    {
        public AsmToken(int asmLine, AsmTokens token, string value)
        {
            AsmLine = asmLine + 1;
            Token = token;
            Value = value;
        }

        public AsmToken(int asmLine, AsmTokens token, char value)
        {
            AsmLine = asmLine + 1;
            Token = token;
            Value = new string(value, 1);
        }

        public AsmToken(int asmLine, AsmTokens token, int value)
        {
            AsmLine = asmLine + 1;
            Token = token;
            IntValue = value;
        }

        public AsmTokens Token { get; private set; }
        public string Value { get; private set; }
        public int IntValue { get; private set; }
        public int AsmLine { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return "[" + Token + " - '" + Value + "']";
            return "[" + Token + " - '" + IntValue + "']";
        }
    }
}