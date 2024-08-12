namespace Compiler
{
    public static class Processor
    {
        public static Dictionary<string, List<IdentifierExpression>> ParamsRequiered = new Dictionary<string, List<IdentifierExpression>>();
        public static Dictionary<string, EffectInstance> Effects = new Dictionary<string, EffectInstance>();

        private static string? FindName(List<IdentifierExpression> expressions)
        {
            var expression = expressions.FirstOrDefault(expr => expr is IdentifierExpression id &&
                                                                 (id.Value.Type == TokenType.Name || 
                                                                  id.Value.Type == TokenType.EffectParam));
            if (expression != null)
            {
                expressions.Remove(expression);
                return (IdentifierExpression)expression.Value;
            }

            return null;
        }

        public static EffectInstance FindEffect(List<IdentifierExpression> expressions)
        {
            string? name = FindName(expressions);
            ValidateEffectName(name);

            if (InternalFinder(ParamsRequiered[name], expressions))
            {
                return Effects[name];
            }
            else
            {
                throw new InvalidOperationException("Unexpected code Entrance");
            }
        }

        public static void SetParameters(List<IdentifierExpression> values, List<Expression> parameters)
        {
            foreach (var ex in parameters.OfType<BinaryExpression>())
            {
                var identifierExpression = ex.Left as IdentifierExpression;
                if (identifierExpression == null) continue;

                var match = values.FirstOrDefault(id => id.Value.Meaning == identifierExpression.Value.Meaning);
                if (match != null)
                {
                    identifierExpression.Result = match.Result;
                    ex.Result = match.Result;
                }
            }
        }

        public static void UpdateScope(Expression expression, Scope scope)
        {
            if (scope != null && expression is IdentifierExpression ide && 
                expression.CheckType != ValueType.Card && expression.CheckType != ValueType.Context && 
                expression.CheckType != ValueType.CardCollection)
            {
                scope.AddVar(ide);
            }
        }

        private static void ValidateEffectName(string? name)
        {
            if (name == null)
                throw new ArgumentNullException("Evaluate Error, There is no name given for the Effect of the Card");
            if (!Effects.ContainsKey(name))
                throw new KeyNotFoundException($"Evaluate Error, there is no effect named {name} declared previously");
        }

        private static bool InternalFinder(List<IdentifierExpression> declared, List<IdentifierExpression> asked)
        {
            if (declared.Count != asked.Count)
                throw new InvalidOperationException($"You must declare exactly {declared.Count} params at the effect, you declared {asked.Count}");

            bool allMatch = asked.All(ask => declared.Any(dec => dec.Equals(ask) && dec.CheckSemantic == ask.CheckSemantic));

            if (!allMatch)
                throw new InvalidOperationException("The params you declared don't coincide with the effect");

            return true;
        }
    }
}
