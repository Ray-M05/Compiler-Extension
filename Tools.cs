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
    
}

