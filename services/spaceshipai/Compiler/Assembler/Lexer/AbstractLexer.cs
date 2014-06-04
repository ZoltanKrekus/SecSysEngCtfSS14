using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpaceshipAI.Compiler.Assembler.Lexer
{
    public abstract class AbstractLexer<TT>
    {
        private readonly string _input;

        protected AbstractLexer(string input)
        {
            _input = input;
            Eof = false;
            P = -1;
        }

        /// <summary>
        /// The current character
        /// </summary>
        protected char C { get; private set; }

        /// <summary>
        /// The current position in the string
        /// </summary>
        protected int P { get; private set; }

        /// <summary>
        /// End of file
        /// </summary>
        public bool Eof { get; private set; }

        /// <summary>
        /// Index of the current line
        /// </summary>
        public int CurrentLine { get; protected set; }

        /// <summary>
        /// Offset in the current line
        /// </summary>
        public int CurrentCol { get; protected set; }

        public void Reset()
        {
            P = -1;
            Eof = false;
            Consume();
        }

        public bool Consume()
        {
            if (P >= _input.Length - 1)
            {
                Eof = true;
                return false;
            }
            P++;
            CurrentCol++;
            C = _input[P];
            if (C == '\n')
            {
                CurrentLine++;
                CurrentCol = 0;
            }
            return true;
        }

        public event EventHandler NewLine;

        protected virtual void OnNewLine()
        {
            EventHandler handler = NewLine;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected void ThrowError(string message, int line = -1, int col = -1)
        {
            if (line == -1)
                line = CurrentLine;
            if (col == -1)
                col = CurrentCol;
            throw new LexicalErrorException(string.Format("Lexical error: {0} - In line {1}, at {2}.",
                message, line, col));
        }

        protected void InvalidCharacter()
        {
            ThrowError(string.Format("Invalid character '{0}' (#{1})",
                C, (int) C));
        }

        public abstract IEnumerable<TT> GetTokens();
    }

    [Serializable]
    public class LexicalErrorException : Exception
    {
        public LexicalErrorException(string message) : base(message)
        {
        }


        protected LexicalErrorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}