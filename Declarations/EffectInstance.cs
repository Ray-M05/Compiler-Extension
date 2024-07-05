namespace Compiler;

// public interface IEffect_ 
// {
//     Expression? Name { get; set; }
//     Expression? Params { get; set; }
//     Expression? Action { get; set; }
// }

public class EffectInstance: Expression
{
    public Expression? Name  {get; set;}
    public Expression? Params { get; set; }
    public Expression? Action { get; set; }
    public override object Evaluate()
    {
        throw new NotImplementedException();
    }
}