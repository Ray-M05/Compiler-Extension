namespace Compiler;
    public class Program
    {
        public static void Main(string[] args)
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
            else if (node is Terminal numberNode)
            {
                Console.WriteLine(new string(' ', indentLevel * 4) + $"Value: {numberNode.ValueForPrint}");
            }
            else if (node is ProgramExpression prognode)
            {
                foreach(EffectInstance eff in prognode.Effects)
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

                }
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
            else if(node is AtomExpression unaryOperator)
            PrintExpressionTree(unaryOperator.Operand, indentLevel + 1);
        }
    }
