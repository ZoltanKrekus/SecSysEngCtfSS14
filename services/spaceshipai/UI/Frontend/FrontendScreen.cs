using System.IO;
using System.Text;

namespace SpaceshipAI.UI.Frontend
{
    public abstract class FrontendScreen
    {
        protected readonly StreamReader Reader;
        protected readonly StreamWriter Writer;

        protected FrontendScreen(StreamReader reader, StreamWriter writer)
        {
            Reader = reader;
            Writer = writer;
        }

        public string ReadLine()
        {
            string str = Reader.ReadLine();
            if (str == null)
                throw new IOException("Client disconnected");
            return str.Trim();
        }

        public string ReadUntilEot()
        {
            var sb = new StringBuilder();
            int nextChar;
            while ((nextChar = Reader.Read()) != -1 && nextChar != 4 && nextChar != 26 && sb.Length < 2048)
            {
                sb.Append((char) nextChar);
            }
            if (nextChar == -1)
                throw new IOException("Connection closed.");
            if (sb.Length == 4096)
            {
                WriteLine("Maximum capacity reached...");
            }
            return sb.ToString();
        }

        public void Write(string text)
        {
            Writer.Write(text);
            Writer.Flush();
        }

        public void WriteLine(string line)
        {
            Writer.WriteLine(line);
            Writer.Flush();
        }

        public abstract void Start();
    }
}