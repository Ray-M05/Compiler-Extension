namespace Compiler;

public abstract class Expression
{
public string? printed;
public ValueType? CheckType;
public Scope SemScope;
public object? Result; 
public virtual void Print(int indentLevel = 0)
{
    if(CheckType!= null)
    Console.WriteLine(new string(' ', indentLevel * 4) + "Token " + printed+ "--- Type"+ CheckType);
    else
    Console.WriteLine(new string(' ', indentLevel * 4) + "Token " + printed);
}
public abstract ValueType? CheckSemantic(Scope scope);
public abstract object Evaluate(Scope scope, object set, object instance=null);
}
public class ProgramExpression: Expression
{
    public List<Expression> Instances;
    public ProgramExpression()
    {
        Instances= new();
    }


    public override object Evaluate(Scope scope, object set, object instance=null)
    {
        List<Card> cards= new(false);
        object values=null;
        foreach(Expression exp in Instances)
        {
            values= exp.Evaluate(scope, null, instance);
            if(exp is CardInstance card)
            {
                cards.Add((Card)values);
            }
        }
        return cards;
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
        if (Tools.GetOperatorType(Operator) != null)
        {
            var type = Tools.GetOperatorType(Operator);
            if(Left.CheckSemantic(scope)  == type && Right.CheckSemantic(scope) == type)
            {
                Left.CheckType = type;
                Right.CheckType =type;
                if(Tools.GetPrecedence.ContainsKey(Operator) && Tools.GetPrecedence[Operator] == 2)
                    return ValueType.Bool;
                else
                    return type;
            }
            else
               Errors.List.Add(new CompilingError($"Expected {Tools.GetOperatorType(Operator)} Type as an {Operator} argument",new Position()));
        }

        else if (Operator == TokenType.Equal)
        {
            Left.CheckType = Left.CheckSemantic(scope);
            Right.CheckType = Right.CheckSemantic(scope);
            if(Left.CheckType == Right.CheckType)
            {
                return ValueType.Bool;
            }
            else
               Errors.List.Add(new CompilingError("Expected the same type in both sides of the equal operator",new Position()));
        }

        else if(Operator == TokenType.Index)
        {
            if(Left.CheckType!= ValueType.CardCollection)
            {
                if(Left.CheckSemantic(scope)== ValueType.CardCollection) //TODO: no se puede preguntar junto r y l?
                {
                    Left.CheckType= ValueType.CardCollection;
                }
                else
                    Errors.List.Add(new CompilingError("Expected a CardCollection as a left argument of the Index operator",new Position()));
            }
            if(Right.CheckSemantic(scope)== ValueType.Int)
            {
                Right.CheckType= ValueType.Int;
                return ValueType.Card;
            }
            else
                Errors.List.Add(new CompilingError("Expected an Int as a right argument of the Index operator",new Position()));
        }

        else if(Operator == TokenType.Colon || Operator== TokenType.Assign)
        {
            Right.CheckType= Right.CheckSemantic(scope);
            ValueType? tempforOut;
            object v;
            if(scope == null||!scope.Find(Left, out tempforOut, out v) || !scope.WithoutReps) 
            {
                Left.CheckType= Left.CheckSemantic(scope);
                if(Tools.VariableTypes.Contains(Left.CheckType))
                { 
                    if(Left.CheckType == Right.CheckType || Left.CheckType== ValueType.Unassigned)
                    {
                        Left.CheckType= Right.CheckType;
                        scope?.AddVar(Left);
                    }
                    else 
                        Errors.List.Add(new CompilingError("Expected the same type in both sides of the equal operator",new Position()));
                }
                else 
                    Errors.List.Add(new CompilingError("Expected a variable type in the left side of the equal operator",new Position()));
            }
            else
                Errors.List.Add(new CompilingError("Variable already declared",new Position()));

            CheckType= Right.CheckType;
            return Right.CheckType;
        }

        else if(Operator == TokenType.Point)
        {
            Left.CheckType = Left.CheckSemantic(scope);
            if(Left.CheckType != ValueType.Null && Right is Atom right && Tools.GetPossibleMethods(Left.CheckType).Contains(right.Value.Type))
            {
                Right.CheckType= right.CheckSemantic(scope);
                return Tools.GetKeywordType(right.Value.Type);
            }
            else if(Left.CheckType != ValueType.Null && Right is BinaryExpression binary && binary.Operator== TokenType.Index )
            {
                if(binary.Left is Atom left && Tools.GetPossibleMethods(Left.CheckType).Contains(left.Value.Type))
                {
                    binary.Left.CheckType= Tools.GetKeywordType(left.Value.Type);
                    return binary.CheckSemantic(scope);
                }
                else
                    Errors.List.Add(new CompilingError("Expected a valid method",new Position()));
            }
        }

        else 
           Errors.List.Add(new CompilingError("Unknown operator",new Position()));
            return null;
    }
    public override object Evaluate()
    {

        throw new Exception("Invalid Operator");
        
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
            return atom.Value.Meaning==Value.Meaning;
        }
        return false;
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        throw new NotImplementedException();
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        throw new NotImplementedException();
    }
}

public class UnaryExpression : Atom
{
    public Expression Parameter { get; set; }
    public TokenType Operator { get; set; }

    public Dictionary<TokenType, ValueType> ValueTypers;
    public UnaryExpression(Expression operand, Token Operator):base(Operator)
    {
        Parameter = operand;
        this.Operator = Operator.Type;
        printed= Operator.Type.ToString();
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
        CheckType = Tools.GetKeywordType(Operator);
        return CheckType;
    }
}
public class Number: Atom
{
    public Number(Token token): base(token)
    {
        this.printed= "Number";
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        return Convert.ToDouble(Value.Meaning);
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        CheckType = ValueType.Int; //TODO: this.SemanticScope = scope;
        return CheckType;
    }
}
public class BooleanLiteral : Atom
{
    public BooleanLiteral(Token token): base(token)
    {
        this.printed = "Boolean";
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
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
        this.printed = "ID"; 
    }

    public override ValueType? CheckSemantic(Scope scope)
    {
        if(Tools.GetKeywordType(Value.Type) != null)
        {
            CheckType = Tools.GetKeywordType(Value.Type);
            return CheckType;
        }
        else
        {
            ValueType? type;
            object v;
            if(scope!= null && scope.Find(this, out type, out v))
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

    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        
    }
}
public class StringExpression : Atom
{
    public StringExpression(Token token):base(token)
    {
        this.printed = "STRING"; // O alguna otra forma de representar el identificador visualmente
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        return Value.Meaning.Substring(1,Value.Meaning.Length-2);
    }
    public override ValueType? CheckSemantic(Scope scope)
    {
        CheckType = ValueType.String;
        return CheckType;
    }
}
   