namespace Compiler;
public class EffectInstance: Expression
{
    public Expression? Name  {get; set;}
    public List<Expression>? Params { get; set; }
    public Expression? Action { get; set; }

    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Name!= null)
        Name.CheckSemantic(scope);
        else
        {
            Errors.List.Add(new CompilingError("Effect must have a name", new Position()));
        }
        
        SemScope.WithoutReps= true;
        if (Params != null)
        foreach(var param in Params)
        {
            param.CheckSemantic(SemScope);
        }
        SemScope.WithoutReps= false;


        if(Action!= null && Action.CheckSemantic(SemScope) != ValueType.Checked)
            Errors.List.Add(new CompilingError("Effect must have an action", new Position()));

        return ValueType.Checked;
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

 public class InstructionBlock: Expression
{
    public List<Expression>? Instructions= new();
    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        foreach(var instruction in Instructions)
        {
            ValueType? type = instruction.CheckSemantic(SemScope);
        }
        return ValueType.Checked;
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        Scope Evaluator = new Scope(scope);
        foreach(Expression item in Instructions)
        {
            item.Evaluate(Evaluator,null);
        }
        return true;
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "Instruction Block";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}

public class Action: Expression
{
    public IdentifierExpression? Targets;
    public IdentifierExpression? Context;
    public InstructionBlock? Instructions;
    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Targets!= null)
        {
            SemScope.AddVar(Targets);
            Targets.CheckType= ValueType.CardCollection;
        }
        else
        {
            Errors.List.Add(new CompilingError("Action must have a valid target", new Position()));
        }
        
        if(Context!= null)
        {
            SemScope.AddVar(Context);
            Context.CheckType= ValueType.Context;
        }
        else
        {
            Errors.List.Add(new CompilingError("Action must have a valid context", new Position()));
        }
        
        if(Instructions!= null&& Instructions.CheckSemantic(SemScope)== ValueType.Checked)
        {
            Instructions.CheckType= ValueType.Checked;
        }
        else
        {
            Errors.List.Add(new CompilingError("Action must have valid instructions", new Position()));
        }
        return ValueType.Checked;
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "Action";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}

public class ForExpression: Expression
{
    public InstructionBlock? Instructions= new();
    public IdentifierExpression? Variable;
    public IdentifierExpression? Collection;

    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Variable!= null)
        {
            ValueType? type;
            object v;
            if(!scope.Find(Variable, out type, out v))
            {
                Variable.CheckType= ValueType.Card;
                SemScope.AddVar(Variable);
            }
            else
            {
                Errors.List.Add(new CompilingError("Variable already declarated, isnt available in a for loop", new Position()));
            }
        }
        else
        {
            Errors.List.Add(new CompilingError("For must have a variable", new Position()));
        }
        
        if(Collection!= null&& Collection.CheckSemantic(scope)== ValueType.CardCollection)
        {
            Collection.CheckType= ValueType.CardCollection;
        }
        else
        {
            Errors.List.Add(new CompilingError("For must have a collection", new Position()));
        }
        
        if(Instructions!= null && Instructions.CheckSemantic(SemScope)!= ValueType.Checked)
        {
            Errors.List.Add(new CompilingError("For must have valid instructions", new Position()));
        }
        return  ValueType.Checked;
    }
    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        Scope Evaluator = new Scope(scope);

        Collection!.Result = Collection.Evaluate(scope, null);

        List<Card> list = (List<Card>)Collection.Result; 

        for (int i = 0; i < list.Count; i++)
        {
            Variable!.Result = list[i]; 
            Evaluator.AddVar(Variable, Variable.Value); 
            Instructions!.Evaluate(Evaluator, null); 
        }

        return null;
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "For";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}
public class WhileExpression: Expression
{
    public InstructionBlock? Instructions= new();
    public Expression? Condition;

    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Condition!= null&& Condition.CheckSemantic(scope)== ValueType.Bool)
        {
            Condition.CheckType= ValueType.Bool;
        }
        else
        {
            Errors.List.Add(new CompilingError("While must have a valid condition", new Position()));
        }
        
        if(Instructions!= null && Instructions.CheckSemantic(SemScope)!= ValueType.Checked)
        {
            Errors.List.Add(new CompilingError("While must have valid instructions", new Position()));
        }
        return  ValueType.Checked;
    }

    public override object Evaluate(Scope scope,object set, object instance= null)
    {
        Scope Evaluator = new Scope(scope); 

        while (true)
        {
            if (!(bool)Condition.Evaluate(scope, null))
            {
                break;
            }

            Instructions.Evaluate(Evaluator, null);
        }

        return null;   
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "While";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}

//TODO: esta interfaz puede ser una clase?
public interface IEffect
{
    EffectInstance effect{get; set;}
    List<IdentifierExpression> Params{get; set;}
    Selector Selector{get; set;}

    void Execute(DeckContext context)
    {
        List<CompilerCard> targets= new(null,null);
        if(Selector!= null)
        targets= Selector.Execute(context);
        effect.Execute(context, targets, Params);
    }
}
public class MyEffect: IEffect
{
    public MyEffect(EffectInstance eff, Selector Sel, List<IdentifierExpression> Par)
    {
        effect = eff;
        Selector = Sel;
        Params= Par;
    }
    public List<IdentifierExpression> Params{get; set;}

    public EffectInstance effect{get; set;}

    public Selector Selector{get; set;}
    public override string ToString()
    {
        string s= "Efecto: " + (string)effect.Name.Result + "\n";
        foreach(IdentifierExpression identifier in Params)
        {
            s+= identifier.Value.Meaning+ "\n";
        }
        return s;
    }
}
