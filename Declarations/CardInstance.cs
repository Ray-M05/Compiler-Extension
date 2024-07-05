namespace Compiler;
public class CardInstance: Expression
{
    public Expression? Name;
    public Expression? Type;
    public Expression? Faction;
    public Expression? Power;
    public List<Expression>? Range;
    public List<Expression>? OnActivation;

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}