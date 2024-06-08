using System.Text.RegularExpressions;
namespace Compiler
{
    public class Token 
    {
        public TokenType Type;
        public string Value;
        public (int, int) PositionError;
        public Token(TokenType type, string value, (int, int) positionError){
            Type = type;
            Value = value;
            PositionError = positionError;
        }
    }
    

public class Lexer {
    private string input;
    private List<Token> tokens;

public Dictionary <TokenType, string> keywords = new Dictionary< TokenType, string> {
    {TokenType.LineChange, @"\r"},
    {TokenType.Whitespace, @"\s+"},
    {TokenType.For, @"\bfor\b"},
    {TokenType.While, @"\bwhile\b"},
    {TokenType.Effect, @"\bEffect\b"},
    {TokenType.Card, @"\bCard\b"},
    {TokenType.If, @"\bif\b"},
    {TokenType.ElIf, @"\belif\b"},
    {TokenType.Else, @"\belse\b"},
    {TokenType.Pow, @"\^"},
    {TokenType.Increment, @"\+\+"},
    {TokenType.Decrement, @"\-\-"},
    {TokenType.Plus, @"\+"},
    {TokenType.Minus, @"\-"},
    {TokenType.Multiply, @"\*"},
    {TokenType.Divide, @"\/"},
    {TokenType.And, @"\&\&"},
    {TokenType.Or, @"\|\|"},
    {TokenType.Equal, "=="},
    {TokenType.LessEq, "<="},
    {TokenType.MoreEq, ">="},
    {TokenType.Less, "<"},
    {TokenType.More, ">"},
    {TokenType.SpaceConcatenation, "@@"},
    {TokenType.Concatenation, "@"},
    {TokenType.Assign, "="},
    {TokenType.LParen, @"\("},
    {TokenType.RParen, @"\)"},
    {TokenType.LBracket, @"\["},
    {TokenType.RBracket, @"\]"},
    {TokenType.LCurly, @"\{"},
    {TokenType.RCurly, @"\}"},
    {TokenType.Boolean, @"\b(true|false)\b"},
    {TokenType.Int, @"\b\d+\b"},
    {TokenType.String, "\".*?\""},
    {TokenType.Id, @"\b[A-Za-z_][A-Za-z_0-9]*\b"}
};

    public Lexer(string input) {
        this.input = input;
        this.tokens = new List<Token>();
    }

    public List<Token> Tokenize() 
    {
        while (input.Length!=0) 
        {
                bool isfound = false;
                foreach (TokenType type in keywords.Keys) {
                    string pattern = keywords[type];
                    Match match = Regex.Match(input,"^"+ pattern);
                    if (match.Success) 
                    {
                        Token token = new Token(type, match.Value, (0,0));
                        tokens.Add(token);
                        input= input.Substring(match.Value.Length); 
                        isfound = true;
                        break;
                    }
                }   
                if (!isfound){
                    break;
                }
        }
        return tokens;
    }

}





public enum TokenType {
    Whitespace,
    LineChange,
    For,
    While,
    Effect,
    Card,
    If,
    ElIf,
    Else,
    Pow,
    Increment,
    Decrement,
    Plus,
    Minus,
    Multiply,
    Divide,
    And,
    Or,
    Less,
    More,
    Equal,
    LessEq,
    MoreEq,
    SpaceConcatenation,
    Concatenation,
    Assign,
    LParen,
    RParen,
    LBracket,
    RBracket,
    LCurly,
    RCurly,
    Boolean,
    Int,
    String,
    Id
}

}



