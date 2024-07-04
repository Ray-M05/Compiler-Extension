using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

    private Dictionary< TokenType, Action<object> > Options;
    private Dictionary<TokenType, Func<Expression>> CardParsing;
    private Dictionary<TokenType, Func<Expression>> EffectParsing;


    private void InitializeTools()
    {
        Options = new Dictionary<TokenType, Action<object>>
        {
            { TokenType.Card, CardTreatment},
            { TokenType.Effect, EffectTreatment}
        };

        CardParsing = new Dictionary<TokenType, Func<Expression>>
        {
            { TokenType.Name, NameTreatment() },
            { TokenType.Type,  },
            { TokenType.Faction,  },
            { TokenType.Power,  },
            { TokenType.Range, },
            { TokenType.OnActivation,  },
            { TokenType.PostAction,  } // iria aqui?
        };

        EffectParsing = new Dictionary<TokenType, Func<Expression>>
        {
            { TokenType.Name,  NameTreatment()},
            { TokenType.Params,  },
            { TokenType.Action,  }
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
               LookAhead(token.Type, TokenType.Effect)||LookAhead(token.Type, TokenType.Range)||
               LookAhead(token.Type, TokenType.Faction)||LookAhead(token.Type, TokenType.Power)||
               LookAhead(token.Type, TokenType.OnActivation)||LookAhead(token.Type, TokenType.PostAction))
            {
                CardParsing[tokens[position++].Type].Invoke();
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
            EffectParsing[tokens[position++].Type].Invoke();
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


private void NameTreatment()
{
    // EffectInstance effect = new();
    // if()
    // {
    //  effect.Name = ParseAssignment();   
    // }
}

}