using System.Net;
using FootballLeague.Shared.Exceptions;

namespace FootballLeague.API.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private static readonly string _internalServerError = "InternalServerError";
        private readonly RequestDelegate _next;
        private readonly string _responseContentType = "application/json";

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException e)
            {
                context.Response.StatusCode = e.StatusCode;
                context.Response.ContentType = _responseContentType;

                ErrorDetails errorResponse = new ErrorDetails()
                .SetStatusCode(context.Response.StatusCode)
                .SetMessage(e.Message);

                if (e.Args != null && e.Args.Count > 0)
                {
                    errorResponse = new ErrorDetails()
                    .SetStatusCode(context.Response.StatusCode)
                    .SetMessage(e.Message)
                    .AddArgs(e.Args);
                }

                await context.Response.WriteAsync(errorResponse.ToJson());
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = _responseContentType;

                await context.Response.WriteAsync(new ErrorDetails()
                .SetStatusCode(context.Response.StatusCode)
                .SetMessage(_internalServerError)
                .ToJson());
            }
        }
    }
}