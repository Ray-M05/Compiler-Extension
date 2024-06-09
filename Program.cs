namespace Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string s = "a vt7tg \n || 23" ;
                Lexer l = new Lexer(s);
                List<Token> tokens = l.Tokenize();
                foreach (Token t in tokens)
                {
                Console.WriteLine(t.Type.ToString()+ " in " + t.PositionError.Item1+" line "+ " and " + t.PositionError.Item2+ " column ");                
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    
    }
}
