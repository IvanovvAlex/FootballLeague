using System.Text.Json;

namespace FootballLeague.Shared.Exceptions
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public List<object>? Args { get; set; }

        public ErrorDetails SetStatusCode(int statusCode)
        {
            StatusCode = statusCode;
            return this;
        }

        public ErrorDetails SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public ErrorDetails AddArgs(params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (Args == null)
                {
                    Args = new List<object>();
                }
                Args.AddRange(args);
            }
            return this;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}