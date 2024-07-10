namespace Compiler;
public class EffectInstance: Expression
{
    public Expression? Name  {get; set;}
    public List<Expression>? Params { get; set; }
    public Expression? Action { get; set; }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}

 public class InstructionBlock: Expression
{
    public List<Expression>? Instructions= new();
    public override object Evaluate()
    {
        throw new NotImplementedException();
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

    public override object Evaluate()
    {
        throw new NotImplementedException();
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

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
    public override void Print(int indentLevel = 0)
    {
        printed = "While";
        Console.WriteLine(new string(' ', indentLevel * 4) + printed);
    }
}