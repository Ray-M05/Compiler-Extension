namespace Compiler;
public class EffectInstance: Expression
{
    public Expression? Name;
    public Expression? Params;
    public Expression? Action;
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}