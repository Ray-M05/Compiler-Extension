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
    
    public static ValueType GetType(TokenType token)
    {
        return token switch
        {
            // Strings
            TokenType.NAME => ValueType.String,
            TokenType.FACTION => ValueType.String,
            TokenType.TYPE => ValueType.String,
            TokenType.STRINGTYPE => ValueType.String,
            TokenType.SOURCE => ValueType.String,
            TokenType.EFFECTASSIGNMENT => ValueType.String,

            // Players
            TokenType.OWNER => ValueType.Player,
            TokenType.TRIGGERPLAYER => ValueType.ListCard,

            // Numbers
            TokenType.POWER => ValueType.Number,
            TokenType.PLUS => ValueType.Number,
            TokenType.MINUS => ValueType.Number,
            TokenType.NUMBERTYPE => ValueType.Number,

            // Predicates
            TokenType.PREDICATE => ValueType.Predicate,

            // Booleans
            TokenType.NOT => ValueType.Boolean,
            TokenType.BOOLEAN => ValueType.Boolean,
            TokenType.SINGLE => ValueType.Boolean,

            // List Cards
            TokenType.DECK => ValueType.ListCard,
            TokenType.DECKOFPLAYER => ValueType.ListCard,
            TokenType.GRAVEYARD => ValueType.ListCard,
            TokenType.GRAVEYARDOFPLAYER => ValueType.ListCard,
            TokenType.FIELD => ValueType.ListCard,
            TokenType.FIELDOFPLAYER => ValueType.ListCard,
            TokenType.HAND => ValueType.ListCard,
            TokenType.HANDOFPLAYER => ValueType.ListCard,
            TokenType.BOARD => ValueType.ListCard,
            TokenType.FIND => ValueType.ListCard,

            // Cards
            TokenType.POP => ValueType.Card,

            // Voids
            TokenType.SENDBOTTOM => ValueType.Void,
            TokenType.PUSH => ValueType.Void,
            TokenType.SHUFFLE => ValueType.Void,
            TokenType.ADD => ValueType.Void,

            _ => throw new ArgumentException("Unknown token type", nameof(token)),
        };
    }

    public static Dictionary GetOperand()
}