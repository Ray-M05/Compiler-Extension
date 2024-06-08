namespace Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string s = "" ;
                Lexer l = new Lexer(s);
                List<Token> tokens = l.Tokenize();
                foreach (Token t in tokens)
                {
                    Console.WriteLine(t.Value + " " + t.Type);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    
    }
}
