using System.Text.RegularExpressions;
namespace Compiler;

    public class Token 
    {
        public TokenType Type;
        public string Meaning;
        public Position PositionError { get; set;}
        public Token(TokenType type, string value, (int, int) positionError){
            Type = type;
            Meaning = value;
            PositionError = new Position { Row = positionError.Item1, Column = positionError.Item2 };
        }
    }

        public struct Position
    {
        public int Row;
        public int Column;
    }
    

public class Lexer { 
    private string input;
    private List<Token> tokens;

    public Dictionary <TokenType, string> Keywords = new Dictionary< TokenType, string> {

    //Lines
    { TokenType.LineChange, @"\r"},
    {TokenType.Whitespace, @"\s+"},

    //Keywords
    {TokenType.Effect, @"\beffect\b"},
    {TokenType.Card, @"\bcard\b"},
    {TokenType.EffectParam, @"\bEffect\b"},
    { TokenType.Name, @"\bName\b" },
    { TokenType.Params, @"\bParams\b" },
    { TokenType.Action, @"\bAction\b" },
    { TokenType.Type, @"\bType\b" },
    { TokenType.Faction, @"\bFaction\b" },
    { TokenType.Power, @"\bPower\b" },
    { TokenType.Range, @"\bRange\b" },
    { TokenType.OnActivation, @"\bOnActivation\b" },
    { TokenType.Selector, @"\bSelector\b" },
    { TokenType.PostAction, @"\bPostAction\b" },
    { TokenType.Source, @"\bSource\b" },
    { TokenType.Single, @"\bSingle\b" },
    { TokenType.Predicate, @"\bPredicate\b" },
    { TokenType.In, @"\bin\b" },
    { TokenType.Hand, @"\bhand\b" },
    { TokenType.Deck, @"\bdeck\b" },
    { TokenType.Board, @"\bboard\b" },
    { TokenType.Context, @"\bcontext\b" },
    { TokenType.TriggerPlayer, @"\bTriggerPlayer\b" },
    { TokenType.Find, @"\bFind\b" },
    { TokenType.Push, @"\bPush\b" },
    { TokenType.SendBottom, @"\bSendBottom\b" },
    { TokenType.Pop, @"\bPop\b" },
    { TokenType.Remove, @"\bRemove\b" },
    { TokenType.Shuffle, @"\bShuffle\b" },
    { TokenType.Owner, @"\bOwner\b" },
    { TokenType.NumberType, @"\bNumber\b" },
    { TokenType.StringType, @"\bString\b" },

    //Booleans
    { TokenType.True, @"\btrue\b" },
    { TokenType.False, @"\bfalse\b" },
    {TokenType.For, @"\bfor\b"},
    {TokenType.While, @"\bwhile\b"},
    {TokenType.If, @"\bif\b"},
    {TokenType.ElIf, @"\belif\b"},
    {TokenType.Else, @"\belse\b"},
    {TokenType.Not, @"!" },
    {TokenType.And, @"\&\&"},
    {TokenType.Or, @"\|\|"},


    //Operators
    {TokenType.Pow, @"\^"},
    { TokenType.PlusEqual, @"+=" },
    { TokenType.MinusEqual, @"-=" },
    {TokenType.Increment, @"\+\+"},
    {TokenType.Decrement, @"\-\-"},
    {TokenType.Plus, @"\+"},
    {TokenType.Minus, @"\-"},
    {TokenType.Multiply, @"\*"},
    {TokenType.Divide, @"\/"},
    {TokenType.Equal, "=="},
    {TokenType.LessEq, "<="},
    {TokenType.MoreEq, ">="},
    {TokenType.Less, "<"},
    {TokenType.More, ">"},

    //Symbols
    {TokenType.SpaceConcatenation, "@@"},
    {TokenType.Concatenation, "@"},
    {TokenType.Assign, "="},
    { TokenType.Colon, @":" },
    { TokenType.Comma, @"," },        
    { TokenType.Semicolon, @";" },
    { TokenType.Arrow, @"=>" },
    {TokenType.LParen, @"\("},
    {TokenType.RParen, @"\)"},
    {TokenType.LBracket, @"\["},
    {TokenType.RBracket, @"\]"},
    {TokenType.LCurly, @"\{"},
    {TokenType.RCurly, @"\}"},

    //Identifiers
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
        int row = 0;
        int column = 0;
        while (input.Length!=0) 
        {
                bool isfound = false;
                foreach (TokenType type in Keywords.Keys) {
                    string pattern = Keywords[type];
                    Match match = Regex.Match(input,"^"+ pattern);
                    if (match.Success) 
                    {
                        if(type!= TokenType.Whitespace && type!= TokenType.LineChange)
                        {
                            Token token = new Token(type, match.Value, (row,column));
                            tokens.Add(token);
                        }
                        if(type== TokenType.LineChange)
                        {
                            row++;
                            column =0;
                        }
                        input= input.Substring(match.Value.Length); 
                        column+= match.Value.Length;
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

    //Lines
    Whitespace,
    LineChange,

    //Operators
    For,
    While,
    True,
    False,
    If,
    ElIf,
    Else,
    Pow,
    Increment,
    Decrement,
    Plus,
    Minus,
    PlusEqual,
    MinusEqual,
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
    Int,
    String,
    Id,
    Colon,
    Comma,
    Semicolon,
    Not,

    //Keywords
    Effect,
    EffectParam,
    Card,
    Name,
    Params,
    Action,
    Type,
    Faction,
    Power,
    Range,
    OnActivation,
    Selector,
    PostAction,
    Source,
    Single,
    Predicate,
    In,
    Hand,
    Owner,
    Deck,
    Board,
    Context,
    TriggerPlayer,
    Find,
    Push,
    SendBottom,
    Pop,
    Remove,
    Shuffle,
    NumberType,
    StringType,
    Arrow,
}





