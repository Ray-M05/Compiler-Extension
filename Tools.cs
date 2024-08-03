namespace Compiler;
public static class Tools
{
    public static bool EqualTerm(object left, object right)
    {
        if(left is int _left && right is int _right)
        {
            return _left== _right;
        }
        else if(left is bool _leftb && right is bool _rightb)
        {
            return _leftb== _rightb;
        }
        else if(left is string _lefts && right is string _rights)
        {
            return _lefts== _rights;
        }
        return false;
    }

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
            TokenType.LessEq => ValueType.Bool,
            TokenType.MoreEq => ValueType.Bool,
            TokenType.Less => ValueType.Bool,
            TokenType.More => ValueType.Bool,
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
                ValueType.CardCollection => new[] { TokenType.Push, TokenType.SendBottom, TokenType.Pop, TokenType.Shuffle }.ToList(),
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