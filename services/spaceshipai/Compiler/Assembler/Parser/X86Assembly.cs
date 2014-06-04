using System.Collections.Generic;
using System.Linq;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    public class X86Assembly
    {
        private readonly Dictionary<string, int> _constants;
        private readonly Dictionary<string, byte[]> _data;
        private readonly HashSet<string> _labels;
        private readonly Dictionary<string, string> _nameDictionary;
        private readonly List<X86AssemblySection> _sections;
        private int _constIndex = 1;
        private int _dataIndex = 1;
        private int _labelIndex = 1;

        public X86Assembly()
        {
            _constants = new Dictionary<string, int>();
            _data = new Dictionary<string, byte[]>();
            _sections = new List<X86AssemblySection>();
            _labels = new HashSet<string>();
            _nameDictionary = new Dictionary<string, string>();
        }

        public Dictionary<string, int> Constants
        {
            get { return _constants.ToDictionary(x => InternalNameOf(x.Key), x => x.Value); }
        }

        public Dictionary<string, byte[]> Data
        {
            get { return _data.ToDictionary(x => InternalNameOf(x.Key), x => x.Value); }
        }

        public IEnumerable<X86AssemblySection> Sections
        {
            get { return _sections.AsEnumerable(); }
        }

        public string InternalNameOf(string name)
        {
            return _nameDictionary[name];
        }

        public void AddConstant(string name, int value)
        {
            _constants.Add(name, value);
            _nameDictionary.Add(name, NextConstName());
        }

        public void AddData(string name, byte[] data)
        {
            _nameDictionary.Add(name, NextDataName());
            _data.Add(name, data);
        }

        public void AddSection(X86AssemblySection section)
        {
            _labels.Add(section.Name);
            _sections.Add(section);
            if (section.Name == "_start")
                _nameDictionary.Add("_start", "_start");
            else
                _nameDictionary.Add(section.Name, NextLabel());
        }

        public bool LabelExists(string label)
        {
            return _labels.Contains(label);
        }

        public bool ConstantExists(string constant)
        {
            return _constants.ContainsKey(constant);
        }

        public bool DataExists(string constant)
        {
            return _data.ContainsKey(constant);
        }

        public bool NameExists(string name)
        {
            return LabelExists(name) || ConstantExists(name) || DataExists(name);
        }

        public string NextLabel()
        {
            return "label" + (_labelIndex++);
        }

        public string NextConstName()
        {
            return "const" + (_constIndex++);
        }

        public string NextDataName()
        {
            return "data" + (_dataIndex++);
        }

        public int SectionCount
        {
            get { return _sections.Count; }
        }
    }

    public class X86AssemblySection
    {
        private readonly LinkedList<X86Instruction> _instructions;

        public X86AssemblySection(string name)
        {
            Name = name;
            _instructions = new LinkedList<X86Instruction>();
        }

        public X86AssemblySection() : this("_start")
        {
            DefaultLabel = true;
        }

        public string Name { get; private set; }

        /// <summary>
        /// The default label used at the beginning of the code piece.
        /// Deleted if empty.
        /// </summary>
        public bool DefaultLabel { get; private set; }

        public int InstructionCount
        {
            get { return _instructions.Count; }
        }

        public IEnumerable<X86Instruction> Instructions
        {
            get { return _instructions.AsEnumerable(); }
        }

        public void AddInstruction(X86Instruction instruction)
        {
            _instructions.AddLast(instruction);
        }
    }
}