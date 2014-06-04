using System.Text;

namespace SpaceshipAI.Compiler.CSharpGeneration
{
    public class ProgramSource
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public string Source
        {
            get { return _sb.ToString(); }
        }

        public void AddLine(string text, int indentation = 0)
        {
            _sb.Append("\t\t\t");
            for (int i = 0; i < indentation; i++)
                _sb.Append("\t");
            _sb.AppendLine(text);
        }

        public void StartLine(string text, int indentation = 0)
        {
            _sb.Append("\t\t\t");
            for (int i = 0; i < indentation; i++)
                _sb.Append("\t");
            _sb.Append(text);
        }

        public void Append(string text)
        {
            _sb.Append(text);
        }

        public void EndLine(string text = "")
        {
            _sb.AppendLine(text);
        }

        public void AddCodeLine(params string[] text)
        {
            _sb.Append("\t\t\t");
            foreach (string s in text)
            {
                _sb.Append(s);
            }
            _sb.AppendLine(";");
        }

        public void AddJump(string condition, string label)
        {
            _sb.Append("\t\t\t");
            _sb.AppendLine("if (" + condition + ")");
            _sb.Append("\t\t\t\t");
            _sb.Append("goto ");
            _sb.Append(label);
            _sb.AppendLine(";");
        }
    }
}