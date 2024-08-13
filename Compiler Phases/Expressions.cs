using System.Reflection;

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
        List<Card> cards= new();
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
    public override object Evaluate(Scope scope, object set, object instance=null)
    {
        if (Operator != TokenType.Point&&Operator != TokenType.Index
        &&Operator != TokenType.Assign&&Operator != TokenType.Colon
        &&Operator != TokenType.PlusEqual&&Operator != TokenType.MinusEqual)
        {
            Right.Result= Right.Evaluate(scope,set,instance);
            Left.Result= Left.Evaluate(scope,set,instance); 

            switch(Operator)
            {
                case TokenType.Plus:
                this.Result= (int)Left.Result + (int)Right.Result;
                break;

                case TokenType.Minus:
                this.Result= (int)Left.Result - (int)Right.Result;
                break;

                case TokenType.Multiply:
                this.Result= (int)Left.Result * (int)Right.Result;
                break;

                case TokenType.Divide:
                if((int)Right.Result ==0)
                    Errors.List.Add(new CompilingError("Division by zero",new Position()));
                else    
                    this.Result= (int)Left.Result / (int)Right.Result;
                break;

                case TokenType.Pow:
                this.Result= Math.Pow((int)Left.Result, (int)Right.Result);
                break;

                case TokenType.Less:
                this.Result= (int)Left.Result < (int)Right.Result;
                break;

                case TokenType.More:
                this.Result= (int)Left.Result > (int)Right.Result;
                break;

                case TokenType.And:
                this.Result= (bool)Left.Result && (bool)Right.Result;
                break;

                case TokenType.Or:
                this.Result= (bool)Left.Result || (bool)Right.Result;
                break;

                case TokenType.Concatenation:
                this.Result= Left.Result.ToString() + Right.Result.ToString();
                break;

                case TokenType.SpaceConcatenation:
                this.Result= Left.Result.ToString() + " " + Right.Result.ToString();
                break;

                case TokenType.Equal:
                this.Result= Left.Result.Equals(Right.Result);
                break;

                case TokenType.LessEq:
                this.Result= (int)Left.Result <= (int)Right.Result;
                break;

                case TokenType.MoreEq:
                this.Result= (int)Left.Result >= (int)Right.Result;
                break;
            }

            return this.Result!;
        }
        else if(Operator== TokenType.PlusEqual)
        {
            Right.Result= Right.Evaluate(scope,set,instance);
            object Result= Left.Evaluate(scope, set,instance);
            Left.Evaluate(scope,(int)Result! + (int)Right.Result);
            this.Result= Left.Result;
            return Left.Result;   
        }
        else if(Operator== TokenType.MinusEqual)
        {
            Right.Result= Right.Evaluate(scope,set,instance);
            Result= Left.Evaluate(scope, set,instance);
            Left.Evaluate(scope,(int)Result! - (int)Right.Result);
            this.Result= Left.Result;
            return Left.Result;   
        }
        else if(Operator==TokenType.Index)
        {
            Right.Result= Right.Evaluate(scope,set,instance);
            Left.Result= Left.Evaluate(scope, set,instance);
            if(Right.Result is List<Card> list)
            {
                if((int)Right.Result<0|| (int)Right.Result>=list.Count)
                    Errors.List.Add(new CompilingError("Index out of range",new Position()));

                return list[(int)Right.Result];
            }
            else
                Errors.List.Add(new CompilingError("Expected a CardCollection",new Position()));
        }
        else if(Operator== TokenType.Assign || Operator == TokenType.Colon)
        {
            Right.Result= Right.Evaluate(scope, null);
            Left.Evaluate(scope, Right.Result);
            Result= Left.Result;
            return Left.Result;
        }
        else if(Operator== TokenType.Point)
        {
            Left.Result= Left.Evaluate(scope, null!, instance);
            Right.Result= Right.Evaluate(scope, set, Left.Result);
            
            return Right.Result;
        }
        else
            Errors.List.Add(new CompilingError("Unknown operator",new Position()));
        return null;
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
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        if(Parameter!=null) 
        switch(Operator)
        {
            case TokenType.Add:
            case TokenType.Remove:
            case TokenType.Push:
            case TokenType.SendBottom:
            {
                Type type = instance.GetType();
                string methodName = Operator.ToString();
                MethodInfo methodInfo = type.GetMethod(methodName);
                object evaluatedOperand = Parameter.Evaluate(scope, null);
                methodInfo.Invoke(instance, new object[] { evaluatedOperand });
                return null!;
            }

            case TokenType.HandOfPlayer:
            case TokenType.DeckOfPlayer:
            case TokenType.GraveYardOfPlayer:
            case TokenType.FieldOfPlayer:
            case TokenType.Find:
            case TokenType.Pop:
            {
                Type type = instance.GetType();
                string methodName = Operator.ToString();
                MethodInfo methodInfo = type.GetMethod(methodName);
                object evaluatedOperand = Parameter.Evaluate(scope, null);
                Result = methodInfo.Invoke(instance, new object[] { evaluatedOperand });
                return Result!;
            }

            case TokenType.RDecrement:
            Parameter.Result= (int)Parameter.Evaluate(scope,null)-1;
            Result= (int)Parameter.Result+1;
            Processor.UpdateScope(Parameter, scope);
            return (int)Result;

            case TokenType.LDecrement:
            Parameter.Result= (int)Parameter.Evaluate(scope,null)-1;
            Result= (int)Parameter.Result;
            Processor.UpdateScope(Parameter, scope);
            return (int)Result;
            
            case TokenType.RIncrement:
            Parameter.Result= (int)Parameter.Evaluate(scope,null)+1;
            Result= (int)Parameter.Result-1;
            Processor.UpdateScope(Parameter, scope);
            return (int)Result;
            
            case TokenType.LIncrement:
            Parameter.Result= (int)Parameter.Evaluate(scope,null)+1;
            Result= (int)Parameter.Result;
            Processor.UpdateScope(Parameter, scope);
            return (int)Result;

            case TokenType.Minus:
            Result= (int)Parameter.Evaluate(scope, null)*-1;
            return (int)Result-1;

            case TokenType.Plus:
            Result= Parameter.Evaluate(scope, null);
            return Result;
            
            case TokenType.Not:
            {
                Result= Parameter.Evaluate(scope, null);
                return !(bool)Result;
            }
        }
        if(Tools.GetOperatorType(Operator)!=null)
        {
            CheckType = Tools.GetOperatorType(Operator);
            return CheckType;
        }
        throw new Exception("Invalid Unary Operator");
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
        if (Tools.GetPossibleMethods(ValueType.Context)!= null)
        {
            if(instance is DeckContext context)
            {
                var cont = context.GetType();
                return cont.GetProperty(Value.Meaning).GetValue(context);
            }
            else 
                Errors.List.Add(new CompilingError("Expected a context",new Position()));
        }
        if(Tools.GetPossibleMethods(ValueType.Card)!= null)
        {
            if(instance is Card card)
            {
                if(set!=null)
                {
                    string propertyName = Value.Meaning;
                    PropertyInfo property = card.GetType().GetProperty(propertyName);
                    property.SetValue(card, set);
                }
                else
                {
                    return card.GetType().GetProperty(Value.Meaning).GetValue(card);
                }
            }
            
        }
        else if(set!= null)
        {
            Result= set;
            if(scope!=null)
            scope.AddVar(this);
            return Value;
        }
        else
        {
            object value= null;
            if(Value!= null)
            {
                ValueType? v;
                if(scope!=null)
                scope.Find(this, out v, out value);
            }
            return value;
        }
        return null;
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
   