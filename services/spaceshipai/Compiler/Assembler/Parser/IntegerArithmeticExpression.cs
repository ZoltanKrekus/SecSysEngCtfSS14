using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpaceshipAI.Compiler.Assembler.Parser
{
    /// <summary>
    /// Holds an internal platform-free representation of an arithmetic expression.
    /// It does NOT check for validity.
    /// </summary>
    public class IntegerArithmeticExpression
    {
        private readonly LinkedList<IntegerArithmeticExpressionMember> _expressionMembers =
            new LinkedList<IntegerArithmeticExpressionMember>();

        public IEnumerable<IntegerArithmeticExpressionMember> Members
        {
            get { return _expressionMembers.AsEnumerable(); }
        }

        protected void AddMember(IntegerArithmeticExpressionMember member)
        {
            _expressionMembers.AddLast(member);
        }

        public void AddOperator(IntegerArithmeticOperator op)
        {
            AddMember(op);
        }

        public void AddOperator(IntegerArithmeticOperatorType op)
        {
            AddOperator(new IntegerArithmeticOperator(op));
        }

        public void AddTerm(IntegerArithmeticExpressionTerm term)
        {
            AddMember(term);
        }

        public void AddTerm(IntegerArithmeticExpressionTermType termType, string value)
        {
            AddTerm(new IntegerArithmeticExpressionTerm(termType, value));
        }

        public void AddTerm(string value)
        {
            AddTerm(new IntegerArithmeticExpressionTerm(IntegerArithmeticExpressionTermType.Register, value));
        }

        public void AddTerm(int value)
        {
            AddTerm(new IntegerArithmeticExpressionTerm(IntegerArithmeticExpressionTermType.Constant,
                value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <summary>
    /// Abstract arithmetic expression member class
    /// </summary>
    public abstract class IntegerArithmeticExpressionMember
    {
    }

    public class IntegerArithmeticExpressionTerm : IntegerArithmeticExpressionMember
    {
        public IntegerArithmeticExpressionTerm(IntegerArithmeticExpressionTermType type, string value)
        {
            Type = type;
            Value = value;
        }

        public IntegerArithmeticExpressionTermType Type { get; private set; }
        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }
    }

    public enum IntegerArithmeticExpressionTermType
    {
        Constant,
        Register
    }

    public class IntegerArithmeticOperator : IntegerArithmeticExpressionMember
    {
        public IntegerArithmeticOperator(IntegerArithmeticOperatorType type)
        {
            Type = type;
        }

        public IntegerArithmeticOperatorType Type { get; private set; }

        public override string ToString()
        {
            switch (Type)
            {
                case IntegerArithmeticOperatorType.Addition:
                    return "+";
                case IntegerArithmeticOperatorType.Division:
                    return "/";
                case IntegerArithmeticOperatorType.Multiplication:
                    return "*";
                case IntegerArithmeticOperatorType.Substraction:
                    return "-";
            }
            return "";
        }
    }

    public enum IntegerArithmeticOperatorType
    {
        Addition,
        Substraction,
        Multiplication,
        Division
    }
}