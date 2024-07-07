namespace Compiler;
public class CardInstance: Expression
{
    public Expression? Name {get; set;}
    public Expression? Type {get; set;}
    public Expression? Faction {get; set;}
    public Expression? Power {get; set;}
    public List<Expression>? Range {get; set;}
    public OnActivation? OnActivation {get; set;}

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class Predicate: Expression
{
    public IdentifierExpression? Unit;
    public Expression? Condition;
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}
public class OnActivation: Expression
{
    public List<EffectParam>? Effects= new();
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
