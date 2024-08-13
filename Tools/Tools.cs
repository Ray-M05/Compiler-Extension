namespace Compiler;
public static class Tools
{
    public static Dictionary<TokenType, int> GetPrecedence = new()
    {
        { TokenType.And, 1 },
        { TokenType.Or, 1 },
        { TokenType.Equal, 2 },
        { TokenType.LessEq, 2 },
        { TokenType.MoreEq, 2 },
        { TokenType.More, 2 },
        { TokenType.Less, 2 },
        { TokenType.Plus, 3 },
        { TokenType.Minus, 3 },
        { TokenType.Concatenation, 3 },
        { TokenType.SpaceConcatenation, 3 },
        { TokenType.Multiply, 4 },
        { TokenType.Divide, 4 },
        { TokenType.Not, 4 },
        { TokenType.Pow, 5 },
        { TokenType.Point, 6 }
    };
    
    public static ValueType? GetKeywordType(TokenType token)
    {
        return token switch
        {
            // Strings
            TokenType.Name => ValueType.String,
            TokenType.Faction => ValueType.String,
            TokenType.Type => ValueType.String,
            TokenType.StringType => ValueType.String,
            TokenType.Source => ValueType.String,
            TokenType.EffectParam => ValueType.String,

            // Players
            TokenType.Owner => ValueType.Player,
            TokenType.TriggerPlayer => ValueType.CardCollection,

            // Numbers
            TokenType.Power => ValueType.Int,
            TokenType.Plus => ValueType.Int,
            TokenType.Minus => ValueType.Int,
            TokenType.RIncrement => ValueType.Int,
            TokenType.LIncrement => ValueType.Int,
            TokenType.RDecrement => ValueType.Int,
            TokenType.LDecrement => ValueType.Int,
            TokenType.NumberType => ValueType.Int,

            // Predicates
            TokenType.Predicate => ValueType.Predicate,

            // Booleans
            TokenType.Not => ValueType.Bool,
            TokenType.Bool => ValueType.Bool, //arreglar lexer de bool como token
            TokenType.Single => ValueType.Bool,

            // List Cards
            TokenType.Deck => ValueType.CardCollection,
            TokenType.DeckOfPlayer => ValueType.CardCollection,
            TokenType.GraveYard => ValueType.CardCollection,
            TokenType.GraveYardOfPlayer => ValueType.CardCollection,
            TokenType.Field => ValueType.CardCollection,
            TokenType.FieldOfPlayer => ValueType.CardCollection,
            TokenType.Hand => ValueType.CardCollection,
            TokenType.HandOfPlayer => ValueType.CardCollection,
            TokenType.Board => ValueType.CardCollection,
            TokenType.Find => ValueType.CardCollection,

            // Cards
            TokenType.Pop => ValueType.Card,

            // Voids
            TokenType.SendBottom => ValueType.Void,
            TokenType.Push => ValueType.Void,
            TokenType.Shuffle => ValueType.Void,
            TokenType.Add => ValueType.Void,
            TokenType.Remove => ValueType.Void,

            _ => null,
        };
    }

    public static ValueType? GetOperatorType(TokenType token)
    {
        return token switch
        {
            TokenType.PlusEqual => ValueType.Int,
            TokenType.MinusEqual => ValueType.Int,
            TokenType.Plus => ValueType.Int,
            TokenType.Minus => ValueType.Int,
            TokenType.Multiply => ValueType.Int,
            TokenType.Divide => ValueType.Int,
            TokenType.Pow => ValueType.Int,
            TokenType.LessEq => ValueType.Int,
            TokenType.MoreEq => ValueType.Int,
            TokenType.Less => ValueType.Int,
            TokenType.More => ValueType.Int,
            TokenType.And => ValueType.Bool,
            TokenType.Or => ValueType.Bool,
            TokenType.Concatenation => ValueType.String,
            TokenType.SpaceConcatenation => ValueType.String,
            _ => null,
        };
    }

    public static List<TokenType> GetPossibleMethods(ValueType? value)
        {
            return value switch
            {
                ValueType.Card => new[] { TokenType.Name, TokenType.Owner, TokenType.Power, TokenType.Faction, TokenType.Range, TokenType.Type }.ToList(),
                ValueType.Context => new[] { TokenType.Find, TokenType.Deck, TokenType.DeckOfPlayer, TokenType.GraveYard, TokenType.GraveYardOfPlayer, TokenType.Field, TokenType.FieldOfPlayer, TokenType.Hand, TokenType.HandOfPlayer, TokenType.Board, TokenType.TriggerPlayer }.ToList(),
                ValueType.CardCollection => new[] { TokenType.Push, TokenType.SendBottom, TokenType.Pop, TokenType.Shuffle, TokenType.Add, TokenType.Remove }.ToList(),
                _ => new(),
            };
        }

    public static List<ValueType?> VariableTypes= new List<ValueType?>
    {
        ValueType.Int,
        ValueType.Bool,
        ValueType.Unassigned,
        ValueType.String
    };
}