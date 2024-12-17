namespace FootballLeague.Shared.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; } = 500;
        public List<object>? Args { get; set; }

        public AppException(string message) : base(message) { }

        public AppException SetStatusCode(int statusCode)
        {
            StatusCode = statusCode;
            return this;
        }

        public AppException AddArg(params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                Args = new List<object>(args);
            }

            Args.AddRange(args);

            return this;
        }
    }
}