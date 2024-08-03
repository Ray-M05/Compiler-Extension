namespace Compiler;

public abstract class Expression
{
public string? printed;
public ValueType? CheckType;
public Scope SemScope;
public virtual void Print(int indentLevel = 0)
{
    Console.WriteLine(new string(' ', indentLevel * 4) + printed);
}
public abstract ValueType? CheckSemantic(Scope scope);
public abstract object Evaluate();
}
public class ProgramExpression: Expression
{
    public List<Expression> Instances;
    public ProgramExpression()
    {
        Instances= new();
    }


    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        foreach(var instance in Instances)
        {
            if(instance.CheckSemantic(scope)!= ValueType.Checked)
            {
                Errors.List.Add(new CompilingError("Semantic Error at the Program",new Position()));
            }
        }
        return ValueType.Void;
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

    public override bool Equals(object? obj)
    {
        if(obj is BinaryExpression bin)
        {
            return bin.Left.Equals(Left) && bin.Right.Equals(Right) && bin.Operator == Operator;
        }
        return false;
    }

    public override ValueType? CheckSemantic(Scope scope)
    {
        
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
public class Atom: Expression
{
    public string? ValueForPrint;
    public Token Value { get; }
    public Atom(Token token)
    {
        this.ValueForPrint = token.Meaning;
        Value= token;
    }

    public override bool Equals(object? obj)
    {
        if(obj is Atom atom)
        {
            return atom.Value.Equals(Value);
        }
        return false;
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        throw new NotImplementedException();
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class UnaryExpression : Expression
{
    public Expression Parameter { get; set; }
    public TokenType Operator { get; set; }

    public Dictionary<TokenType, ValueType> ValueTypers;
    public UnaryExpression(Expression operand, TokenType Operator)
    {
        Parameter = operand;
        this.Operator = Operator;
        ValueTypers = new()
        {
            { TokenType.SendBottom ,ValueType.Card },
            { TokenType.Remove ,ValueType.Card },
            { TokenType.Push ,ValueType.Card },
            { TokenType.Add ,ValueType.Card },
            { TokenType.HandOfPlayer ,ValueType.Player },
            { TokenType.DeckOfPlayer ,ValueType.Player },
            { TokenType.GraveYardOfPlayer ,ValueType.Player },
            { TokenType.FieldOfPlayer ,ValueType.Player },
            { TokenType.RDecrement ,ValueType.Int },
            { TokenType.LDecrement ,ValueType.Int },
            { TokenType.RIncrement ,ValueType.Int },
            { TokenType.LIncrement ,ValueType.Int },
            { TokenType.Not ,ValueType.Bool },
            { TokenType.Find, ValueType.Predicate}
        };
    }
    public override object Evaluate()
    {
        switch(Operator)
        {
            case TokenType.Not:
                return !(bool)Parameter.Evaluate();
            case TokenType.Minus:
                return -1* (double)Parameter.Evaluate();
            default:
            throw new Exception("Unknown unary operator");
        }
    }
        
    public override ValueType? CheckSemantic(Scope scope)
    {
        if(Parameter!= null&& ValueTypers.ContainsKey(Operator))
        {
            ValueType type = ValueTypers[Operator];
            if(Parameter.CheckSemantic(scope)!= type)
                Errors.List.Add(new CompilingError($"Expected {type} Type as an {Operator} argument",new Position()));
            else
                Parameter.CheckType= type;
        }
        CheckType = Tools.GetType(Operator);
        return CheckType;
    }
}
public class Number: Atom
{
    public Number(Token token): base(token)
    {
        this.printed= "Number";
    }
    public override object Evaluate()
    {
        return Convert.ToDouble(Value.Meaning);
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        CheckType = ValueType.Int;
        return CheckType;
    }
}
public class BooleanLiteral : Atom
{
    public BooleanLiteral(Token token): base(token)
    {
        this.printed = "Boolean";
    }
    public override object Evaluate()
    {
        return Convert.ToBoolean(Value.Meaning);
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        CheckType = ValueType.Bool;
        return CheckType;
    }
}

 

public class IdentifierExpression : Atom
{
    public IdentifierExpression(Token token):base(token)
    {
        this.printed = "ID"; // O alguna otra forma de representar el identificador visualmente
    }

    public override ValueType? CheckSemantic(Scope scope)
    {
        if(Tools.GetType(Value.Type) != null)
        {
            CheckType = Tools.GetType(Value.Type);
            return CheckType;
        }
        else
        {
            ValueType? type;
            if(scope!= null && scope.Find(this, out type))
            {
                CheckType= type;
                return type;
            }
            else
            {
                CheckType= ValueType.Unassigned;
                return ValueType.Unassigned;
            }
        }
    }

}
public class StringExpression : Atom
{
    public StringExpression(Token token):base(token)
    {
        this.printed = "STRING"; // O alguna otra forma de representar el identificador visualmente
    }
    public override object Evaluate()
    {
        return Value.Meaning.Substring(1,Value.Meaning.Length-2);
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        CheckType = ValueType.String;
        return CheckType;
    }
}
   