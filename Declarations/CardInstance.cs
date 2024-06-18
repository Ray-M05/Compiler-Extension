namespace Compiler;
public class CardInstance: Expression
{
    public Expression? Name;
    public Expression? Type;
    public Expression? Effect;
    public Expression? Faction;
    public Expression? Power;
    public List<Expression>? Range;
    public List<Expression>? OnActivation;
    public List<Expression>? PostAction;

    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}