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

                foreach (CompilingError item in Errors.List)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(parser.position);
                PrintExpressionTree(root);
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
                foreach(EffectInstance eff in prognode.Instances)
                {
                    PrintExpressionTree(eff, indentLevel + 1);
                }
                foreach(CardInstance card in prognode.Cards)
                {
                    PrintExpressionTree(card, indentLevel + 1);
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
            PrintExpressionTree(unaryOperator.Operand, indentLevel + 1);

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
