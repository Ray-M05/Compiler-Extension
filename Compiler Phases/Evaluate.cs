namespace Compiler;

public class Evaluator
{
    public Evaluator(Expression expression)
    {
        root = expression;
    }
    Expression root;
    public object Evaluate()
    {
        return root.Evaluate();
    }
}
