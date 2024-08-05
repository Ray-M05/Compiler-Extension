namespace Compiler;
public class CardInstance: Expression
{
    
    public Expression? Name {get; set;}
    public Expression? Type {get; set;}
    public Expression? Faction {get; set;}
    public Expression? Power {get; set;}
    public List<Expression>? Range {get; set;}
    public OnActivation? OnActivation {get; set;}

    public override ValueType? CheckSemantic(Scope scope)
    {
        printed= "Card";
        SemScope = new Scope(scope);
        if(Name!= null)
            Name.CheckSemantic(scope);
        else
        {
            Errors.List.Add(new CompilingError("Card must have a name", new Position()));
        }
        if(Type!= null)
            Type.CheckSemantic(scope);
        else
        {
            Errors.List.Add(new CompilingError("Card must have a type", new Position()));
        }
        if(Faction!= null)
            Faction.CheckSemantic(scope);
        else
        {
            Errors.List.Add(new CompilingError("Card must have a faction", new Position()));
        }     
        //FIXME:arrgelar el parser para que sea opcional
        if(Power!= null)
            Power.CheckSemantic(scope);
        
        //FIXME:
        if(Range!= null)
        foreach(var range in Range)
        {
            range.CheckSemantic(scope);
        }

        if(OnActivation!= null)
        OnActivation.CheckSemantic(scope);
        else
        {
            Errors.List.Add(new CompilingError("Card must have an OnActivation", new Position()));
        }
        return ValueType.Checked;
    }

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class OnActivation: Expression
{
    public List<EffectParam>? Effects= new();

    public override ValueType? CheckSemantic(Scope scope)
    {
        printed= "OnActivation";
        if(Effects!= null)
        foreach(var effect in Effects)
        {
            effect.CheckSemantic(SemScope);
        }
        return ValueType.Checked;
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "OnActivacion";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}
public class EffectParam: Expression
{
    public List<Expression>? Effect = new();
    public Selector? Selector;
    public EffectParam? PostAction;

    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        SemScope.WithoutReps= true;
        if(Effect!= null)
        foreach(var effect in Effect)
        {
            effect.CheckSemantic(SemScope);
        }
        else
        {
            Errors.List.Add(new CompilingError("Effect must have a name", new Position()));
        }
        SemScope.WithoutReps= false;
        if(!(Selector!= null && Selector.CheckSemantic(SemScope) == ValueType.Checked))
        {
            Errors.List.Add(new CompilingError("Effect must have a selector", new Position()));
        }
        
        if(PostAction!= null)
        {
            if(!(PostAction.CheckSemantic(SemScope) == ValueType.Checked))
            {
                Errors.List.Add(new CompilingError("Effect must have a valid post action", new Position()));
            }
        }
        return ValueType.Checked;
    }

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "OnActivacion";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}
public class Selector: Expression
{
    public Expression? Source;
    public Expression? Single;
    public Expression? Predicate;

    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Source!= null && Source.CheckSemantic(scope) == ValueType.String)
        {
            Source.CheckType = ValueType.String;
        }
        else
        {
            Errors.List.Add(new CompilingError("Selector must have a source", new Position()));
        }
        if(Single!= null && Single.CheckSemantic(scope) == ValueType.Bool)
        {
            Single.CheckType = ValueType.Bool;
        }
        else
        {
            Errors.List.Add(new CompilingError("Selector must have a single", new Position()));
        }
        if(!(Predicate!= null && Predicate.CheckSemantic(scope) == ValueType.Predicate))
        Errors.List.Add(new CompilingError("Selector must have a predicate", new Position()));
        
        return ValueType.Checked;
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "Selector";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}

public class Predicate: Expression
{
    public IdentifierExpression? Unit;
    public Expression? Condition;
    public override ValueType? CheckSemantic(Scope scope)
    {
        SemScope = new Scope(scope);
        if(Unit!= null)
        {
            ValueType? type;
            if(!scope.Find(Unit, out type))
            {
                Unit.CheckType= ValueType.Card;
                SemScope.AddVar(Unit);
            }
        }
        else
        {
            Errors.List.Add(new CompilingError("Predicate most have a unit or unit already declarated", new Position()));
        }
        if(Condition!= null && Condition.CheckSemantic(scope)== ValueType.Bool)
        {
        Condition.CheckType = ValueType.Bool;
        }
        else
        {
            Errors.List.Add(new CompilingError("Predicate must have a valid condition", new Position()));
        }
    return ValueType.Predicate;
        
    }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}