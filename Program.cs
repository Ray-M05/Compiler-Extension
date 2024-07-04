namespace Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string s = "";
                Lexer l = new Lexer(s);
                List<Token> tokens = l.Tokenize();
                foreach (Token t in tokens)
                {
                Console.WriteLine(t.Type.ToString()+ " in " + t.PositionError.Row+" line "+ " and " + t.PositionError.Column+ " column ");                
                }
                Parser parser = new(tokens);
                Expression root = parser.Parse();
                Console.WriteLine(parser.position);
                PrintExpressionTree(root);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


         static void PrintExpressionTree(Expression node, int indentLevel = 0)
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

            }

        }
    
    }
}
