using System;
using System.Linq;
using System.Reflection;


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
            p.Instances.Add(ParseCard());
        }
    }
    
    private void EffectTreatment(object program)
    {
        if(program is ProgramExpression p)
        {
            p.Instances.Add(ParseEffect());
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
        try
        {
            Expression expression;
            expression = ParseGeneral();
            return expression;
        }
        catch(Exception e)
        {
            Console.WriteLine("Roto en: " + position);
            Console.WriteLine(e);
        }
        return null;
    }

    private Expression ParseGeneral()
    {
        ProgramExpression general= new();
        Token token = tokens[position];
        while(position< tokens.Count)
        {
            if(LookAhead(token.Type, TokenType.Effect) || LookAhead(token.Type, TokenType.Card))
            {
                if(LookAhead(tokens[++position].Type, TokenType.LCurly))
                {
                    position++;
                    Options[token.Type].Invoke(general);
                    if(position< tokens.Count)
                    token = tokens[position];
                }
                else
                Errors.List.Add(new CompilingError("Expected Left Curly, invalid token",token.PositionError));
            }
            else
            Errors.List.Add(new CompilingError("Only expects cards or effects",token.PositionError));
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
            Errors.List.Add(new CompilingError("Invalid token, expecting properties of cards or Right Curly",token.PositionError));
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
            Errors.List.Add(new CompilingError("Invalid token, expecting properties of effects or Right Curly",token.PositionError));;
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

    public Expression ParseExpression(int parentprecedence =0)
    {
        var left = ParsePrimaryExpression();

        while (position < tokens.Count)
        {
            int precedence;
            if(Tools.GetPrecedence.ContainsKey(tokens[position].Type))
            precedence = Tools.GetPrecedence[tokens[position].Type];
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
        if (position >= tokens.Count)
        throw new Exception("Unexpected end of input");
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
            return new UnaryExpression(operand, unary);
        }
        else if (LookAhead(tokens[position].Type, TokenType.Shuffle)||LookAhead(tokens[position].Type, TokenType.Pop))
        {
            Token token= tokens[position];
            if(LookAhead(tokens[++position].Type, TokenType.LParen) && LookAhead(tokens[++position].Type, TokenType.RParen))
            {
                position++;
                return new UnaryExpression(null, token.Type);
            }
        }
        else if (LookAhead(tokens[position].Type, TokenType.Push) ||LookAhead(tokens[position].Type, TokenType.SendBottom)
                ||LookAhead(tokens[position].Type, TokenType.Remove)||LookAhead(tokens[position].Type, TokenType.HandOfPlayer)
                ||LookAhead(tokens[position].Type, TokenType.DeckOfPlayer)||LookAhead(tokens[position].Type, TokenType.Add)
                ||LookAhead(tokens[position].Type, TokenType.FieldOfPlayer)||LookAhead(tokens[position].Type, TokenType.GraveYardOfPlayer))
                {
                    Token token= tokens[position];
                    if(LookAhead(tokens[++position].Type, TokenType.LParen))
                    {
                        position++;
                        Expression argument = ParseExpression();
                        if(LookAhead(tokens[position++].Type, TokenType.RParen))
                        {
                            return new UnaryExpression(argument, token.Type);
                        }
                    }
                }
        throw new Exception("Not recognizable primary token");
    }

    private Expression ParsePropertyAssignment()
    {
        Expression left;
        left= ParsePrimaryExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;

            if (LookAhead(token.Type, TokenType.Assign)|| LookAhead(token.Type, TokenType.Colon))
            {
                position++;
                right = ParseExpression();
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(LookAhead(token.Type, TokenType.Increment)|| LookAhead(token.Type, TokenType.Decrement))
            {
                position++;
                right = new UnaryExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(LookAhead(token.Type, TokenType.PlusEqual)|| LookAhead(token.Type, TokenType.MinusEqual))
            {
                position++;
                right = new UnaryExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
        
        
        if(LookAhead(tokens[position].Type, TokenType.Comma) || LookAhead(tokens[position].Type,TokenType.Semicolon)||LookAhead(tokens[position].Type,TokenType.RCurly))
        {
            if(!LookAhead(tokens[position].Type,TokenType.RCurly))
                position++;
            if(Binary!= null)
                return Binary;
            else
                throw new Exception();
        }
        else
        Errors.List.Add(new CompilingError("Unexpected Comma or Semicolon",token.PositionError));
        return null;
    }

    private Expression ParseParamAssigment()
    {
        Expression left;
        left= ParsePrimaryExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;
        if (LookAhead(token.Type, TokenType.Assign)|| LookAhead(token.Type, TokenType.Colon))//Agregar formas como incremento etc...
        {
            position++;
            if(LookAhead(tokens[position].Type,TokenType.NumberType) || LookAhead(tokens[position].Type,TokenType.StringType))
            {
                right = new IdentifierExpression(tokens[position]);
                position++;
                Binary= new BinaryExpression(left, right,token.Type);
                if(LookAhead(tokens[position].Type,TokenType.Comma) || LookAhead(tokens[position].Type,TokenType.Semicolon)
                ||LookAhead(tokens[position].Type,TokenType.RCurly))
                {
                    if(!LookAhead(tokens[position].Type,TokenType.RCurly))
                        position++;
                    if(Binary!= null)
                        return Binary;
                    else
                        throw new Exception();
                }
            }
        }
        Errors.List.Add(new CompilingError("Unexpected Comma or Semicolon",token.PositionError));
        return null;
    }

    private Expression ParseInstructionAssigment()
    {
        Expression left;
        left = ParseExpression();
        Token token= tokens[position];
        Expression right=null;
        Expression Binary= null;

            if (LookAhead(token.Type, TokenType.Assign)|| LookAhead(token.Type, TokenType.Colon))
            {
                position++;
                right = ParseExpression();
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(LookAhead(token.Type, TokenType.Increment)|| LookAhead(token.Type, TokenType.Decrement))
            {
                position++;
                right = new UnaryExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
            else if(LookAhead(token.Type, TokenType.PlusEqual)|| LookAhead(token.Type, TokenType.MinusEqual))
            {
                position++;
                right = new UnaryExpression(ParseExpression(), token.Type);
                Binary= new BinaryExpression(left, right,token.Type);
            }
        
        
        if(LookAhead(tokens[position].Type,TokenType.Comma) || LookAhead(tokens[position].Type,TokenType.Semicolon)||LookAhead(tokens[position].Type,TokenType.RCurly))
        {
            if(!LookAhead(tokens[position].Type,TokenType.RCurly))
                position++;
            if(Binary!= null)
                return Binary;
            else
                return left;
        }
        else
        {
            Errors.List.Add(new CompilingError("Unexpected Comma or Semicolon",token.PositionError));
            return null;
        }
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
                        {
                            if(LookAhead(tokens[position].Type, TokenType.Comma)||
                              LookAhead(tokens[position].Type, TokenType.Semicolon)
                            ||LookAhead(tokens[position].Type, TokenType.RCurly))
                            {
                                if(!LookAhead(tokens[position].Type, TokenType.RCurly))
                                {
                                    position++;
                                }
                                break;
                            }
                            else
                                Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
                        }
                    }
                    else
                        Errors.List.Add(new CompilingError("Invalid token, expecting Comma",tokens[position].PositionError));
                }
                else
                    Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
            }
            return ranges;
        }
        else
            {Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
            return null;}
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
                parameters.Add(ParseParamAssigment());
                token = tokens[position];
            }
            
            else if(LookAhead(tokens[position++].Type, TokenType.RCurly))
            {
                if(LookAhead(tokens[position++].Type, TokenType.Semicolon)||
                   LookAhead(tokens[position-1].Type, TokenType.Comma))
                break;
            }
            else
            Errors.List.Add(new CompilingError("Invalid params definition",token.PositionError));
        }
        else
        Errors.List.Add(new CompilingError("Invalid token",token.PositionError));
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
            else if(LookAhead(tokens[position].Type, TokenType.RCurly))
            {
                if(LookAhead(tokens[++position].Type, TokenType.Semicolon)||
                   LookAhead(tokens[position].Type, TokenType.Comma)){
                        position++;
                        break;
                   }
            }
            else
            Errors.List.Add(new CompilingError("Invalid effect parameter definition",token.PositionError));
        }
        else
        Errors.List.Add(new CompilingError("Invalid token",token.PositionError));
        return parameters;
    }

    private Action ParseAction()
    {
        Action Action = new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.Colon) )
        {
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
        Errors.List.Add(new CompilingError("Invalid Action declaration",tokens[position].PositionError));
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
                    Errors.List.Add(new CompilingError("Expected Colon in effect parameter definition",tokens[position].PositionError));
                    break;
                case TokenType.RCurly:
                    if(LookAhead(tokens[++position].Type,TokenType.Comma)|| 
                       LookAhead(tokens[position].Type,TokenType.Semicolon)||
                       LookAhead(tokens[position].Type,TokenType.RBracket))
                    {
                        if(!LookAhead(tokens[position].Type,TokenType.RBracket))
                        position++;
                    }
                    return effect;
                default:
                    {Errors.List.Add(new CompilingError("Expected card property",tokens[position].PositionError));
                    return null;}
            }
        }
        return effect;
    }


    private InstructionBlock ParseInstructionBlock(bool single= false)
    {
        InstructionBlock block = new();
        do
        {
            if(LookAhead(tokens[position].Type,TokenType.Id))
            {
                block.Instructions.Add(ParseInstructionAssigment());
            }
            else if(LookAhead(tokens[position].Type,TokenType.For))
            {
                block.Instructions.Add(ParseFor());
            }
            else if(LookAhead(tokens[position].Type,TokenType.While))
            {
                block.Instructions.Add(ParseWhile());
            }
            else if(LookAhead(tokens[position].Type, TokenType.RCurly))
            {
                position++;
                break;
            }
            else
            Errors.List.Add(new CompilingError("Invalid instruction definition",tokens[position].PositionError));
        }
        while(true && !single);
        return block;
    }

    private Selector ParseSelector()
    {
        Selector selector= new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.Colon) && LookAhead(tokens[position++].Type, TokenType.LCurly))
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
                    if(LookAhead(tokens[++position].Type,TokenType.Comma)|| LookAhead(tokens[position].Type,TokenType.Semicolon)|| LookAhead(tokens[position].Type, TokenType.RCurly))
                    {
                        if(!LookAhead(tokens[position].Type, TokenType.RCurly))
                        position++;
                        return selector;
                    }
                    else
                    {
                        Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
                        break;
                    }

                default:
                    {Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
                    return null;}
            }
        }
        return selector;
    }

    public Predicate ParsePredicate()
    {
        if(LookAhead(tokens[++position].Type, TokenType.Colon))
        {
            Predicate predicate= new();
            if(LookAhead(tokens[++position].Type, TokenType.LParen) && LookAhead(tokens[++position].Type, TokenType.Id))
                predicate.Unit= new IdentifierExpression(tokens[position]);
                if(LookAhead(tokens[++position].Type, TokenType.RParen) && LookAhead(tokens[++position].Type, TokenType.Arrow))
                {
                    position++;
                    predicate.Condition= ParseExpression();
                    if(LookAhead(tokens[position].Type, TokenType.Comma)|| LookAhead(tokens[position].Type, TokenType.RCurly))
                    {
                        if(LookAhead(tokens[position].Type, TokenType.Comma))
                        position++;
                        return predicate;
                    }
                    else
                        Errors.List.Add(new CompilingError("Expected Comma",tokens[position].PositionError));
                }
                else
                    Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
        }
        else
            Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
            return null;
    }


    private ForExpression ParseFor()
    {
        ForExpression ForExp = new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.Id))
        {
            ForExp.Variable = new IdentifierExpression(tokens[position-1]);
            if(LookAhead(tokens[position++].Type, TokenType.In) && LookAhead(tokens[position++].Type, TokenType.Id))
            {
                ForExp.Collection= new IdentifierExpression(tokens[position-1]);
                if(LookAhead(tokens[position++].Type, TokenType.LCurly))
                {
                    ForExp.Instructions=ParseInstructionBlock();
                    if(LookAhead(tokens[position].Type, TokenType.Comma)||LookAhead(tokens[position].Type, TokenType.Semicolon))
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
                Errors.List.Add(new CompilingError("Invalid token",tokens[position].PositionError));
            }
        }
        else
        Errors.List.Add(new CompilingError("Invalid for declaration statement",tokens[position].PositionError));
        return ForExp;
    }
    private WhileExpression ParseWhile()
    {
        WhileExpression WhileExp = new();
        position++;
        if(LookAhead(tokens[position++].Type, TokenType.LParen))
        {
            WhileExp.Condition = ParseExpression();
            if(LookAhead(tokens[position++].Type, TokenType.RParen))
            {
                if(LookAhead(tokens[position++].Type, TokenType.LCurly))
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
        Errors.List.Add(new CompilingError("Invalid while declaration statement",tokens[position].PositionError));
        return WhileExp;
    }
}