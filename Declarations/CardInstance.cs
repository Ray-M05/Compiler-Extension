namespace Compiler;
public class CardInstance: Expression
{
    public Expression? Name;
    public Expression? Type;
    public Expression? Faction;
    public Expression? Power;
    public List<Expression>? Range;
    public OnActivation? OnActivation;

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
