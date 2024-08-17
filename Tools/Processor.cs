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
                return (string)expression.Result!;
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
                Errors.List.Add(new CompilingError("Unexpected code Entrance", new Position()));
                return null;
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
                Errors.List.Add(new CompilingError("Evaluate Error, There is no name given for the Effect of the Card", new Position()));
            if (!Effects.ContainsKey(name))
                Errors.List.Add(new CompilingError("Evaluate Error, The Effect of the Card is not declared", new Position()));
        }

        private static bool InternalFinder(List<IdentifierExpression> declared, List<IdentifierExpression> asked)
        {
            if (declared.Count != asked.Count)
                Errors.List.Add(new CompilingError("You must declare exactly params at the effect, you declared", new Position()));

            bool allMatch = asked.All(ask => declared.Any(dec => dec.Equals(ask) && dec.CheckType == ask.CheckType));

            if (!allMatch)
                Errors.List.Add(new CompilingError("The params you declared don't coincide with the effect", new Position()));

            return true;
        }
    }
}
