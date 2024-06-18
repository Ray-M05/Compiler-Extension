 namespace Compiler;
    public class CompilingError
    {
        public ErrorCode Code { get; private set; }

        public string Argument { get; private set; }

        public Position Location { get; private set; }

        public CompilingError(Position posError, ErrorCode code, string argument)
        {
            this.Code = code;
            this.Argument = argument;
            Location = posError;
        }
    }

    public enum ErrorCode
    {
        None,
        Expected,
        Invalid,
        Unknown,
    }