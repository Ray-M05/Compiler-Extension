 namespace Compiler;
    public class CompilingError
    {
        public ErrorCode Code { get; private set; }

        public TokenType Tok { get; private set; }

        public Position Location { get; private set; }

        public CompilingError(Position posError, ErrorCode code, TokenType invalidToken)
        {
            this.Code = code;
            this.Tok = invalidToken;
            Location = posError;
        }

        public override string ToString()
        {
            return $"{Code} token {Tok} at Row:{Location.Row}, Column:{Location.Column}";
        }
    }

    public enum ErrorCode
    {
        None,
        Expected,
        Invalid,
        Unknown,
    }