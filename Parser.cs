using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Compiler;

public class Parser
{
    private bool LookAhead(TokenType token, TokenType token1)
    {
        return token == token1;
    }

    public int position;
    List<Token> tokens;
    public bool ExpectingAssigment;
    public bool ExpectingProp;

    private Dictionary< TokenType, Action<object>> Options;
    private Dictionary<string, Action<CardInstance, PropertyInfo>> CardParsing;
    private Dictionary<string, Action<EffectInstance, PropertyInfo>> EffectParsing;


    private void InitializeTools()
    {
        Options = new Dictionary<TokenType, Action<object>>
        {
            { TokenType.Card, CardTreatment},
            { TokenType.Effect, EffectTreatment}
        };

        CardParsing = new Dictionary<string, Action<CardInstance, PropertyInfo>>
        {
            { "Name",  AssignTreatment},
            { "Type", AssignTreatment},
            { "Faction", AssignTreatment },
            { "Power", AssignTreatment },
            { "Range",RangeTreatment },
            { "OnActivation",  OnActivTreatment},
           // { "PostAction",  AssignTreatment} // iria aqui?
        };

        EffectParsing = new Dictionary<string, Action<EffectInstance, PropertyInfo>>
        {
            { "Name", AssignTreatment},
            { "Params",  ParamsTreatment},
            { "Action", ActionTreatment }
        };
    }

    private void CardTreatment(object program)
    {
        if(program is ProgramExpression p)
        {
            p.Cards.Add(ParseCard());
        }
    }
    
    private void EffectTreatment(object program)
    {
        if(program is ProgramExpression p)
        {
            p.Effects.Add(ParseEffect());
        }
    }

    
    public Parser(List<Token> tokens)
    {
        position = 0;
        this.tokens = tokens;

        InitializeTools();
    }
    public Expression Parse()
    {
        Expression expression;
        expression = ParseGeneral();
        return expression;
    }

    public Expression ParseExpression(int parentprecedence =0)
    {
        var left = ParsePrimaryExpression();

        while (position < tokens.Count)
        {
            var precedence = Tools.MagicNumbers[tokens[position].Type];
            if(precedence==0|| precedence<= parentprecedence) 
            break;
            
            var operatortoken = tokens[position++].Type;
            var right = ParseExpression(precedence);
            left = new BinaryExpression(left, right, operatortoken);
        }
        return left;
    }
    public Expression ParsePrimaryExpression()
    {
        if (position >= tokens.Count) throw new Exception("Unexpected end of input");
        if (LookAhead(tokens[position].Type, TokenType.LParen))
        {
            position++;
            Expression expr = ParseExpression(); 
            if (!LookAhead(tokens[position].Type,TokenType.RParen))
            {
                throw new Exception("Missing closing parenthesis");
            }
            position++;
            return expr;
        }
        else if (LookAhead(tokens[position].Type, TokenType.False)|| LookAhead(tokens[position].Type, TokenType.True))
        {
            position++;
            return new BooleanLiteral(tokens[position - 1]);
        }
        else if (LookAhead(tokens[position].Type, TokenType.Id))
        {
            position++; 
            return new IdentifierExpression(tokens[position - 1]);
        }
        else if (LookAhead(tokens[position].Type, TokenType.String))
        {
            position++;
            return new StringExpression(tokens[position - 1]);
        }
        else if (LookAhead(tokens[position].Type, TokenType.Int))
        {
            position++;
            return new Number(tokens[position - 1]);
        }
        else if (LookAhead(tokens[position].Type, TokenType.Not)||LookAhead(tokens[position].Type, TokenType.Plus)||LookAhead(tokens[position].Type, TokenType.Minus) && (position == 0 || !LookAhead(tokens[position - 1].Type, TokenType.Int) && !LookAhead(tokens[position - 1].Type, TokenType.Id)))
        {
            TokenType unary = tokens[position].Type;
            position++;
            Expression operand = ParsePrimaryExpression();
            return new AtomExpression(operand, unary);
        }
        throw new Exception("Not recognizable primary token");
    }

    private Expression ParseAssignment()
    {
        Expression left= new IdentifierExpression(tokens[position++]);
        Token token= tokens[position++];
        if (LookAhead(token.Type, TokenType.Assign)|| LookAhead(token.Type, TokenType.Colon))
        {
            Expression right = ParseExpression();
            if(LookAhead(tokens[position].Type, TokenType.Comma) || LookAhead(tokens[position].Type, TokenType.Semicolon))
            {
                position++;
                return new BinaryExpression(left, right,token.Type);
            }
            throw new Exception($"Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
        }
        else if(LookAhead(token.Type, TokenType.Increment)|| LookAhead(token.Type, TokenType.Decrement))
        {
            Expression right = new AtomExpression(ParseExpression(), token.Type);
            if(LookAhead(tokens[position].Type, TokenType.Comma) || LookAhead(tokens[position].Type,TokenType.Semicolon))
            {
                position++;
                return new BinaryExpression(left, right,token.Type);
            }
            throw new Exception($"Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
        }
        else if(LookAhead(token.Type, TokenType.PlusEqual)|| LookAhead(token.Type, TokenType.MinusEqual))
        {
            Expression right = new AtomExpression(ParseExpression(), token.Type);
            if(LookAhead(tokens[position].Type,TokenType.Comma) || LookAhead(tokens[position].Type,TokenType.Semicolon))
            {
                position++;
                return new BinaryExpression(left, right,token.Type);
            }
            throw new Exception($"Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
        }
        else
        throw new Exception($"Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type})");
    }
    
    private Expression ParseGeneral()
    {
        ProgramExpression general= new();
        Token token = tokens[position];
        while(position< tokens.Count)
        {
            if(LookAhead(token.Type, TokenType.Effect) || LookAhead(token.Type, TokenType.Card))
            {
                if(LookAhead(tokens[position++].Type, TokenType.LBracket))
                {
                    Options[token.Type].Invoke(general);
                }
                else
                throw new Exception($"Invalid token {tokens[position-1]}, expected Left Bracket");
            }
            else
            throw new Exception("Invalid program structure, only expects cards or effects");
        }
        return general;
    }


private CardInstance ParseCard()
    {
        CardInstance card= new();
        Token token = tokens[position];
        while(position< tokens.Count)
        {
            if(LookAhead(token.Type, TokenType.Name)||LookAhead(token.Type, TokenType.Type)||
               LookAhead(token.Type, TokenType.Range)||LookAhead(token.Type, TokenType.Power)||
               LookAhead(token.Type, TokenType.Faction)||LookAhead(token.Type, TokenType.OnActivation))
            {
                var instance = card.GetType();
                var prop = instance.GetProperty(token.Meaning);
                var pars = CardParsing[prop.Name];
                pars.Invoke(card, prop);
                token= tokens[position];
            }
            else if(LookAhead(token.Type, TokenType.RCurly))
            {
                position++;
                return card;
            }
            else
            throw new Exception();
        }
        return card;
    }

private EffectInstance ParseEffect()
{
    EffectInstance effect= new();
    Token token = tokens[position];
    while(position< tokens.Count)
    {
        if(LookAhead(token.Type, TokenType.Name)||
           LookAhead(token.Type, TokenType.Params)||
           LookAhead(token.Type, TokenType.Action))
        {
            var instance = effect.GetType();
            var prop = instance.GetProperty(token.Meaning);
            var pars = EffectParsing[prop.Name];
            pars.Invoke(effect, prop);
            token= tokens[position];
        }
        else if(LookAhead(token.Type, TokenType.RCurly))
        {
            position++;
            return effect;
        }
        else
        throw new Exception();
    }
    return effect;
}


private void AssignTreatment(CardInstance card, PropertyInfo p)
{
    p.SetValue(card, ParseAssignment());
}
private void AssignTreatment (EffectInstance effect, PropertyInfo p)
{
    p.SetValue(effect, ParseAssignment());
}

private void RangeTreatment (CardInstance card, PropertyInfo p)
{
    p.SetValue(card, ParseRanges());
}

private void OnActivTreatment (CardInstance card, PropertyInfo p)
{
    p.SetValue(card, ParseOnActivation());
}

private void ParamsTreatment (EffectInstance effect, PropertyInfo p)
{
    p.SetValue(effect, ParseParams());
}

private void ActionTreatment (EffectInstance effect, PropertyInfo p)
{
    p.SetValue(effect, ParseAction());
}

public List<Expression> ParseRanges()
    {
        if(tokens[++position].Type== TokenType.Colon && tokens[++position].Type== TokenType.LBracket)
        {
            List<Expression> ranges = new();
            position++;
            while(position< tokens.Count)
            {
                if(tokens[position].Type== TokenType.String)
                {
                    ranges.Add(ParseExpression());
                    if(tokens[position].Type== TokenType.Comma|| tokens[position].Type== TokenType.RBracket)
                    {
                        position++;
                        if(tokens[position-1].Type== TokenType.RBracket)
                        break;
                    }
                    else
                        throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected Comma");
                }
                else
                    throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected in ");
            }
            return ranges;
        }
        else
            throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column ");
    }


// private OnActivationExpression ParseOnActivation()
//     {
//         OnActivationExpression activation = new();
//         position++;
//         if(tokens[position++].Type== TokenType.Colon && tokens[position++].Type== TokenType.LBracket)
//         while(position< tokens.Count)
//         {
//             if(tokens[position].Type== TokenType.LCurly)
//             {
//                 activation.Effects.Add(ParseEffectAssignment());
//             }
//             else if(tokens[position].Type== TokenType.RBracket)
//             {
//                 position++;
//                 break;
//             }
//             else
//             throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected ????? in OnActivation");
//         }
//         return activation;
//     }
}
