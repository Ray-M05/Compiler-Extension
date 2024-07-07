 namespace Compiler;

 public abstract class Expression
 {
    public string? printed;
    public virtual void Print(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
    public abstract object Evaluate();
 }
public class ProgramExpression: Expression
{
    public List<EffectInstance> Effects;
    public List<CardInstance> Cards;
    public ProgramExpression()
    {
        Effects= new();
        Cards= new();
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class BinaryExpression : Expression
{
    public Expression Left { get; set; }
    public Expression Right { get; set; }
    public TokenType Operator { get; set; }

    public BinaryExpression(Expression left, Expression right, TokenType Op)
    {
        Left = left;
        Right = right;
        Operator = Op;
        this.printed = Op.ToString();
    }
    public override object Evaluate()
    {
switch(Operator)
{
    case TokenType.Plus:
        return (double)Left.Evaluate() + (double)Right.Evaluate();
    case TokenType.Minus:
        return (double)Left.Evaluate() - (double)Right.Evaluate();
    case TokenType.Multiply:
        return (double)Left.Evaluate() * (double)Right.Evaluate();
    case TokenType.Divide:
        return (double)Left.Evaluate() / (double)Right.Evaluate();
    case TokenType.Pow:
        return Math.Pow((double)Left.Evaluate(), (double)Right.Evaluate());

    case TokenType.Equal:
        return Tools.EqualTerm(Left.Evaluate(), Right.Evaluate());
    case TokenType.LessEq:
        return (double)Left.Evaluate() <= (double)Right.Evaluate();
    case TokenType.MoreEq:
        return (double)Left.Evaluate() >= (double)Right.Evaluate();
    case TokenType.More:
        return (double)Left.Evaluate() > (double)Right.Evaluate();
    case TokenType.Less:
        return (double)Left.Evaluate() < (double)Right.Evaluate();
        
    case TokenType.And:
        return (bool)Left.Evaluate() && (bool)Right.Evaluate();
    case TokenType.Or:
        return (bool)Left.Evaluate() || (bool)Right.Evaluate();

    case TokenType.Concatenation:
        return (string)Left.Evaluate() + (string)Right.Evaluate();
    case TokenType.SpaceConcatenation:
        return (string)Left.Evaluate() +" "+ (string)Right.Evaluate();

    default:
        throw new Exception("Invalid Operator");
} 
}
}
public class Terminal: Expression
{
    public string? ValueForPrint;
    public Token Value { get; }
    public Terminal(Token token)
    {
        this.ValueForPrint = token.Meaning;
        Value= token;
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class AtomExpression : Expression
{
    public Expression Operand { get; set; }
    public TokenType Operator { get; set; }

    public AtomExpression(Expression operand, TokenType Operator)
    {
        Operand = operand;
        this.Operator = Operator;
    }
    public override object Evaluate()
    {
        switch(Operator)
        {
            case TokenType.Not:
                return !(bool)Operand.Evaluate();
            case TokenType.Minus:
                return -1* (double)Operand.Evaluate();
            default:
            throw new Exception("Unknown unary operator");
        }
    }
}
public class Number: Terminal
{
    public Number(Token token): base(token)
    {
        this.printed= "Number";
    }
    public override object Evaluate()
    {
        return Convert.ToDouble(Value.Meaning);
    }
}
public class BooleanLiteral : Terminal
{
    public BooleanLiteral(Token token): base(token)
    {
        this.printed = "Boolean";
    }
    public override object Evaluate()
    {
        return Convert.ToBoolean(Value.Meaning);
    }
}

 

public class IdentifierExpression : Terminal
{
    public IdentifierExpression(Token token):base(token)
    {
        this.printed = "ID"; // O alguna otra forma de representar el identificador visualmente
    }

}
public class StringExpression : Terminal
{
    public StringExpression(Token token):base(token)
    {
        this.printed = "STRING"; // O alguna otra forma de representar el identificador visualmente
    }
    public override object Evaluate()
    {
        return Value.Meaning.Substring(1,Value.Meaning.Length-2);
    }
}
   