namespace Compiler;
    public class Program
    {
        public static void Main()
        {
            try
            {
                string filePath = @"C:\Pro\Compiler-Extension\Example.txt";
                string text = File.ReadAllText(filePath);
                Lexer l = new Lexer(text);
                List<Token> tokens = l.Tokenize();
                Console.WriteLine(text);
                foreach (Token t in tokens)
                {
                    Console.WriteLine(t.Type.ToString()+ " in " + t.PositionError.Row+" line "+ " and " + t.PositionError.Column+ " column ");
                }
                Parser parser = new(tokens);
                Expression root = parser.Parse();

                Errors.PrintAll();
                Console.WriteLine(parser.position);
                PrintExpressionTree(root);
                root.CheckSemantic(null);
                PrintExpressionTree(root);
                Errors.PrintAll();
                List<Card> cards= (List<Card>)root.Evaluate(null!,null!);
                CheckingContext context = new();
                
                foreach (Card item in cards)
                {
                    context.Deck.Add(item);
                }
                if(cards!= null)
                for(int i = cards.Count-1; i>= 0 ; i--) 
                {
                    Card card= cards[i];
                    Console.WriteLine(card.ToString());
                    card.Execute(context);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void PrintExpressionTree(Expression node, int indentLevel = 0)
        {
            node.Print(indentLevel);
            if (node is BinaryExpression binaryNode)
            {
                PrintExpressionTree(binaryNode.Left, indentLevel + 1);
                PrintExpressionTree(binaryNode.Right, indentLevel + 1);
            }
            else if(node is Action action)
            {
                PrintExpressionTree(action.Context,indentLevel+1);
                PrintExpressionTree(action.Targets,indentLevel+1);
                PrintExpressionTree(action.Instructions,indentLevel+1);
            }
            else if (node is Atom numberNode)
            {
                Console.WriteLine(new string(' ', indentLevel * 4) + $"Value: {numberNode.ValueForPrint}");
            }
            else if (node is ProgramExpression prognode)
            {
                foreach(Expression eff in prognode.Instances)
                {
                    PrintExpressionTree(eff, indentLevel + 1);
                }
            }
            else if (node is EffectInstance effNode)
            {
                if(effNode.Name!= null)
                PrintExpressionTree(effNode.Name, indentLevel + 1);
                if(effNode.Params != null)
                {
                    Console.WriteLine(new string(' ', (indentLevel+1) * 4) + $"Params");
                    foreach(Expression param in effNode.Params)
                    PrintExpressionTree(param, indentLevel + 2);
                }
                if(effNode.Action!= null)
                {
                    PrintExpressionTree(effNode.Action);
                }
            }
            else if(node is EffectParam effassign)
            {
                if(effassign.Effect!= null)
                {
                    foreach(Expression exp in effassign.Effect)
                    {
                        PrintExpressionTree(exp, indentLevel +1);
                    }
                }
                if(effassign.Selector!= null)
                {
                    PrintExpressionTree(effassign.Selector,indentLevel+1);
                }
                if(effassign.PostAction!= null)
                {
                    PrintExpressionTree(effassign.PostAction);
                }
            }
            else if(node is OnActivation onact)
            {
                foreach(Expression eff in onact.Effects)
                PrintExpressionTree(eff, indentLevel+1);
            }
            else if (node is CardInstance card)
            {
                if(card.Name!= null)
                PrintExpressionTree(card.Name, indentLevel + 1);
                if(card.Power!= null)
                PrintExpressionTree(card.Power, indentLevel + 1);
                if(card.Type!= null)
                PrintExpressionTree(card.Type, indentLevel + 1);
                if(card.Range != null)
                {
                    Console.WriteLine(new string(' ', (indentLevel+1) * 4) + $"Range");
                    foreach(Expression range in card.Range)
                    PrintExpressionTree(range, indentLevel + 2);
                }
            }
            else if(node is UnaryExpression unaryOperator)
            PrintExpressionTree(unaryOperator.Parameter, indentLevel + 1);

            else if(node is InstructionBlock instructionBlock)
            {
                foreach(Expression exp in instructionBlock.Instructions)
                {
                    PrintExpressionTree(exp, indentLevel + 1);
                }
            }
            
            else if(node is ForExpression forexp)
            {
                PrintExpressionTree(forexp.Variable, indentLevel+1);
                PrintExpressionTree(forexp.Collection,indentLevel +1);
                PrintExpressionTree(forexp.Instructions,indentLevel +1);
            }
            
            
            else if(node is WhileExpression whilexp)
            {
                PrintExpressionTree(whilexp.Condition,indentLevel +1);
                PrintExpressionTree(whilexp.Instructions,indentLevel +1);
            }
            
            
            else if(node is Selector selector)
            {
                PrintExpressionTree(selector.Source,indentLevel+1);
                PrintExpressionTree(selector.Single,indentLevel+1);
                PrintExpressionTree(selector.Predicate,indentLevel+1);
            }
            
            
            else if(node is Predicate predicate)
            {
                PrintExpressionTree(predicate.Unit,indentLevel+1);
                PrintExpressionTree(predicate.Condition,indentLevel+1);
            }
        }
    }
    public class CheckingContext: DeckContext
    {
        public bool Turn{get;}
        public List<Card> Find(Expression exp)
        {
            List<Card> result= new();
            if(exp is Predicate predicate)
            {
                foreach(Card card in Board)
                {
                    predicate.Unit!.Result= card;
                    if((bool)predicate.Condition!.Evaluate(exp.SemScope, null!))
                    {
                        result.Add(card);
                    }
                }
            }
            return result;
        }
        
        public override List<Card> Deck{get;}= new List<Card>
        {
        };
        public override List<Card> OtherDeck{get;}= new List<Card>
        {
        };

        public override List<Card> DeckOfPlayer(Player player)
        {
            if(player.Turn)
            {
                return Deck;
            }
            else
            {
            //Here OtherDeck
                return Deck;
            }
        }
        
        
        public override List<Card> GraveYard{get;}= new List<Card>
        {
        };

        public override List<Card> OtherGraveYard{get;}= new List<Card>
        {
        };

        public override List<Card> GraveYardOfPlayer(Player player)
        {
            if(player.Turn)
            {
                return GraveYard;
            }
            else
            {
                return OtherGraveYard;
            }
        }
        
        public override List<Card> Field{get;}= new List<Card>
        {
        };

        public override List<Card> OtherField{get;}= new List<Card>
        {
        };

        public override List<Card> FieldOfPlayer(Player player)
        {
            if(player.Turn)
            {
                return Field;
            }
            else
            {
                return OtherField;
            }
        }
        
        
        public override List<Card> Hand{get;}= new List<Card>
        {
        };

        public override List<Card> OtherHand{get;}= new List<Card>
        {
        };



        public override List<Card> HandOfPlayer(Player player)
        {
            if(player.Turn)
            {
                return Hand;
            }
            else
            {
                return OtherHand;
            }
        }
        public override List<Card> Board{get;}= new List<Card>
        {
        };

        public override Player TriggerPlayer{get;}
    }
        
