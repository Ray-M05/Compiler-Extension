 namespace Compiler;
    public class CompilingError
    {
        public string Message { get; private set; }

        public Position Location { get; private set; }

        public CompilingError(string message, Position posError)
        {
            this.Message = message;
            Location = posError;
        }

        public override string ToString()
        {
            return $"{Message} at Row:{Location.Row}, Column:{Location.Column}";
        }
    }
    public static class Errors
    {
        public static List<CompilingError> List= new List<CompilingError>();
        public static void PrintAll()
        {
            foreach (CompilingError item in List)
            {
                Console.WriteLine(item);
            }
        }
    }