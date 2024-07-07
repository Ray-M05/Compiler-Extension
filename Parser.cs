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
           // { "PostAction",  AssignTreatment} 
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
            int precedence;
            if(Tools.MagicNumbers.ContainsKey(tokens[position].Type))
            precedence = Tools.MagicNumbers[tokens[position].Type];
            else
            precedence = 0;
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
        else if (LookAhead(tokens[position].Type, TokenType.Name) || LookAhead(tokens[position].Type, TokenType.Type)||
                 LookAhead(tokens[position].Type, TokenType.Faction) ||LookAhead(tokens[position].Type, TokenType.Power)||
                 LookAhead(tokens[position].Type, TokenType.EffectParam) || LookAhead(tokens[position].Type, TokenType.Source)||
                 LookAhead(tokens[position].Type, TokenType.Single)||LookAhead(tokens[position].Type, TokenType.Owner)||
                 LookAhead(tokens[position].Type, TokenType.Deck)||LookAhead(tokens[position].Type, TokenType.GraveYard)||
                 LookAhead(tokens[position].Type, TokenType.Field)||LookAhead(tokens[position].Type, TokenType.Board)||
                 LookAhead(tokens[position].Type, TokenType.Hand))
        {
            position++; 
            return new IdentifierExpression(tokens[position - 1]);
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

    private Expression ParsePropertyAssignment()
    {//true means it expects value, false means it expects ValueType
        Expression left;
        left= ParsePrimaryExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;

            if (token.Type == TokenType.Assign|| token.Type == TokenType.Semicolon)
            {
                position++;
                right = ParseExpression();
                Binary= new BinaryExpression(left, right,token.Type);
            }//NADA DE ESTO ESTÁ HECHO
            else if(token.Type == TokenType.Increment|| token.Type == TokenType.Decrement)
            {
                position++;
                right = new AtomExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(token.Type == TokenType.PlusEqual|| token.Type == TokenType.MinusEqual)
            {
                position++;
                right = new AtomExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
        
        
        if(tokens[position].Type==TokenType.Comma || tokens[position].Type==TokenType.Semicolon||tokens[position].Type==TokenType.RCurly)
        {
            if(tokens[position].Type!=TokenType.RCurly)
                position++;
            if(Binary!= null)
                return Binary;
            else
                throw new Exception();
        }
        else
        throw new Exception($"{position} Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
    }

    private Expression ParseParamAssigment()
    {
        Expression left;
        left= ParsePrimaryExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;
        

            if (token.Type == TokenType.Assign|| token.Type == TokenType.Semicolon)//Agregar formas como incremento etc...
            {
                position++;
                if(tokens[position].Type==TokenType.NumberType || tokens[position].Type==TokenType.StringType)
                {
                    right = new IdentifierExpression(tokens[position]);
                    position++;
                    Binary= new BinaryExpression(left, right,token.Type);
                }
            }
        
        if(tokens[position].Type==TokenType.Comma || tokens[position].Type==TokenType.Semicolon||tokens[position].Type==TokenType.RCurly)
        {
            if(tokens[position].Type!=TokenType.RCurly)
                position++;
            if(Binary!= null)
                return Binary;
            else
                throw new Exception();
        }
        else
        throw new Exception($"{position} Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
    }

    private Expression ParseInstructionAssigment()
    {
        Expression left;
        left = ParseExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;

            if (token.Type == TokenType.Assign|| token.Type == TokenType.Semicolon)
            {
                position++;
                right = ParseExpression();
                Binary= new BinaryExpression(left, right,token.Type);
            }//NADA DE ESTO ESTÁ HECHO
            else if(token.Type == TokenType.Increment|| token.Type == TokenType.Decrement)
            {
                position++;
                right = new AtomExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(token.Type == TokenType.PlusEqual|| token.Type == TokenType.MinusEqual)
            {
                position++;
                right = new AtomExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
        
        
        if(tokens[position].Type==TokenType.Comma || tokens[position].Type==TokenType.Semicolon||tokens[position].Type==TokenType.RCurly)
        {
            if(tokens[position].Type!=TokenType.RCurly)
                position++;
            if(Binary!= null)
                return Binary;
            else
                return left;
        }
        else
        throw new Exception($"{position} Unexpected assign token at {token.PositionError.Row} file and {token.PositionError.Column} column({token.Type}), expected Comma or Semicolon");
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
    p.SetValue(card, ParsePropertyAssignment());
}
private void AssignTreatment (EffectInstance effect, PropertyInfo p)
{
    p.SetValue(effect, ParsePropertyAssignment());
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
        if(LookAhead(tokens[++position].Type, TokenType.Colon) &&
           LookAhead(tokens[++position].Type, TokenType.LBracket))
        {
            List<Expression> ranges = new();
            position++;
            while(position< tokens.Count)
            {
                if(LookAhead(tokens[position].Type, TokenType.String))
                {
                    ranges.Add(ParseExpression());
                    if(LookAhead(tokens[position].Type, TokenType.Comma)||
                       LookAhead(tokens[position].Type, TokenType.RBracket))
                    {
                        position++;
                        if(LookAhead(tokens[position-1].Type, TokenType.RBracket))
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


private OnActivation ParseOnActivation()
    {
        OnActivation activation = new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.Colon) &&
           LookAhead(tokens[position++].Type, TokenType.LBracket))
        while(position< tokens.Count)
        {
            if(LookAhead(tokens[position].Type, TokenType.LCurly))
            {
                activation.Effects.Add(ParseEffectParam());
            }
            else if(LookAhead(tokens[position].Type, TokenType.RBracket))
            {
                position++;
                break;
            }
            else
            throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected ????? in OnActivation");
        }
        return activation;
    }


    private List<Expression> ParseParams()
    {
        List<Expression> parameters = new();
        Token token = tokens[++position];
        if(LookAhead(tokens[position++].Type, TokenType.Colon)&&
           LookAhead(tokens[position++].Type, TokenType.LCurly))
        while(true)
        {
            token= tokens[position];
            if(LookAhead(token.Type,TokenType.Id))
            {
                parameters.Add(ParsePropertyAssignment());
                token = tokens[position];
            }
            
            else if(LookAhead(tokens[position++].Type, TokenType.RCurly))
            {
                if(LookAhead(tokens[position++].Type, TokenType.Semicolon)||
                   LookAhead(tokens[position-1].Type, TokenType.Comma))
                break;
            }
            else
            throw new Exception($"{position} Invalid Token at {token.PositionError.Row} row and {token.PositionError.Column} column expected in Params definition");
        }
        else
        throw new Exception($"{position} Invalid Token at {token.PositionError.Row} row and {token.PositionError.Column} column expected in ");
        return parameters;
    }

        private List<Expression> ParseEffParams()
    {
        List<Expression> parameters = new();
        Token token = tokens[++position];
        if(LookAhead(tokens[position++].Type, TokenType.Colon)&&
           LookAhead(tokens[position++].Type, TokenType.LCurly))
        while(true)
        {
            token= tokens[position];
            if(LookAhead(token.Type,TokenType.Id))
            {
                parameters.Add(ParsePropertyAssignment());
                token = tokens[position];
            }
            else if(LookAhead(token.Type,TokenType.Name))
            {
                parameters.Add(ParsePropertyAssignment());
            }
            else if(LookAhead(tokens[position++].Type, TokenType.RCurly))
            {
                if(LookAhead(tokens[position++].Type, TokenType.Semicolon)||
                   LookAhead(tokens[position-1].Type, TokenType.Comma))
                break;
            }
            else
            throw new Exception($"{position} Invalid Token at {token.PositionError.Row} row and {token.PositionError.Column} column expected in Params definition");
        }
        else
        throw new Exception($"{position} Invalid Token at {token.PositionError.Row} row and {token.PositionError.Column} column expected in ");
        return parameters;
    }

    private Action ParseAction()
    {
        Action Action = new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.Colon) )
        {//Action initial sintaxis
            if(LookAhead(tokens[position++].Type, TokenType.LParen) &&
               LookAhead(tokens[position++].Type, TokenType.Id))
                Action.Targets= new IdentifierExpression(tokens[position-1]);

                if(LookAhead(tokens[position++].Type, TokenType.Comma) &&
                   LookAhead(tokens[position++].Type, TokenType.Id))
                Action.Context = new IdentifierExpression(tokens[position-1]);

                if(LookAhead(tokens[position++].Type, TokenType.RParen) &&
                   LookAhead(tokens[position++].Type, TokenType.Arrow) &&
                   LookAhead(tokens[position++].Type, TokenType.LCurly))
                {
                    Action.Instructions= ParseInstructionBlock();
                }
        }
        else
        throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} on an Action declaration statement");
        return Action;
    }

     private EffectParam ParseEffectParam()
    {
        EffectParam effect= new();
        if(LookAhead(tokens[position++].Type, TokenType.LCurly))
        while(position< tokens.Count)
        {
            switch (tokens[position].Type)
            {
                case TokenType.EffectParam:
                    if(LookAhead(tokens[position+2].Type, TokenType.LCurly))
                        effect.Effect = ParseEffParams();
                    else
                        effect.Effect.Add(ParsePropertyAssignment());
                    break;
                case TokenType.Selector:
                    effect.Selector= ParseSelector();
                    break;
                case TokenType.PostAction:
                    position++;
                    if(LookAhead(tokens[position++].Type, TokenType.Colon))
                    effect.PostAction = ParseEffectParam();// Manejo para TokenType.RANGE
                    else
                    throw new Exception($"Invalid Token at {tokens[position-1].PositionError.Row} row and {tokens[position-1].PositionError.Column} column expected Colon in PostAction statement");
                    break;
                case TokenType.RCurly:
                    if(LookAhead(tokens[++position].Type,TokenType.Comma)|| 
                       LookAhead(tokens[position].Type,TokenType.Semicolon)||
                       LookAhead(tokens[position].Type,TokenType.RCurly))
                    {
                        if(!LookAhead(tokens[position].Type,TokenType.RCurly))
                        position++;
                    }
                    return effect;
                default:
                    throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected card item");
            }
        }
        return effect;
    }


    private InstructionBlock ParseInstructionBlock(bool single= false)
    {//No debuggeado problemas a la hora de parsear Id, diferenciacion entre parseo de asignacion y uso de id para llamar un metodo o usar una propiedad
        InstructionBlock block = new();
        do
        {
            if(tokens[position].Type==TokenType.Id)
            {
                block.Instructions.Add(ParseInstructionAssigment());
            }
            else if(tokens[position].Type==TokenType.For)
            {
                block.Instructions.Add(ParseFor());
            }
            else if(tokens[position].Type==TokenType.While)
            {
                block.Instructions.Add(ParseWhile());
            }
            else if(tokens[position++].Type== TokenType.RCurly)
            {
                break;
            }
            else
            throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected in Instruction Block definition");
        }while(true && !single);
        return block;
    }

    private Selector ParseSelector()
    {
        Selector selector= new();
        position++;
        if(tokens[position++].Type== TokenType.Semicolon&& tokens[position++].Type== TokenType.LCurly)
        while(position< tokens.Count)
        {
            switch (tokens[position].Type)
            {
                case TokenType.Source:
                    selector.Source = ParsePropertyAssignment();
                    break;
                case TokenType.Single:
                    selector.Single= ParsePropertyAssignment();
                    break;
                case TokenType.Predicate:
                    selector.Predicate = ParsePredicate();
                    break;
                case TokenType.RCurly:
                    if(tokens[++position].Type==TokenType.Comma|| tokens[position].Type==TokenType.Semicolon)
                    {
                        position++;
                        return selector;
                    }
                    else
                    throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected in ");
                default:
                    throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected selector item");
            }
        }
        return selector;
    }

    public Predicate ParsePredicate()
    {
        if(tokens[++position].Type== TokenType.Semicolon)
        {
            Predicate predicate= new();
            if(tokens[++position].Type== TokenType.LParen && tokens[++position].Type== TokenType.Id)
                predicate.Unit= new IdentifierExpression(tokens[position]);
                if(tokens[++position].Type== TokenType.RParen && tokens[++position].Type== TokenType.Arrow)
                {
                    position++;
                    predicate.Condition= ParseExpression();
                    if(tokens[position].Type== TokenType.Comma|| tokens[position].Type== TokenType.RCurly)
                    {
                        if(tokens[position].Type== TokenType.Comma)
                        position++;
                        return predicate;
                    }
                    else
                        throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected Comma");
                }
                else
                    throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column expected in ");
        
        }
        else
            throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} column ");
    }


    private ForExpression ParseFor()
    {
        ForExpression ForExp = new();
        position++;
        if( tokens[position++].Type== TokenType.Id )
        {//ForExp initial sintaxis
            ForExp.Variable = new IdentifierExpression(tokens[position-1]);
            if(tokens[position++].Type == TokenType.In && tokens[position++].Type== TokenType.Id)
            {
                ForExp.Collection= new IdentifierExpression(tokens[position-1]);
                if(tokens[position++].Type== TokenType.LCurly)
                {
                    ForExp.Instructions=ParseInstructionBlock();
                    if(tokens[position].Type== TokenType.Comma||tokens[position].Type== TokenType.Semicolon)
                    {
                        position++;
                    }
                }
                else
                {
                    position--;
                    ForExp.Instructions= ParseInstructionBlock(true);
                }
            }
            else
            {
                throw new Exception($"{position} Invalid Token at {tokens[position-1].PositionError.Row} row and {tokens[position-1].PositionError.Column} column expected in ");
            }
        }
        else
        throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} on a For declaration statement");
        return ForExp;
    }
    private WhileExpression ParseWhile()
    {
        WhileExpression WhileExp = new();
        position++;
        if( tokens[position++].Type== TokenType.LParen)
        {//WhileExp initial sintaxis
            WhileExp.Condition = ParseExpression();
            if(tokens[position++].Type == TokenType.RParen)
            {
                if(tokens[position++].Type== TokenType.LCurly)
                {
                    WhileExp.Instructions=ParseInstructionBlock();
                }
                else{
                    position--;
                    WhileExp.Instructions= ParseInstructionBlock(true);
                }
                    
            }
        }
        else
        throw new Exception($"{position} Invalid Token at {tokens[position].PositionError.Row} row and {tokens[position].PositionError.Column} on an Action declaration statement");
        return WhileExp;
    }
}