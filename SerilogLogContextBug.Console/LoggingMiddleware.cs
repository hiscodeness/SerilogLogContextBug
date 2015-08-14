namespace SerilogLogContextBug
{
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Serilog;
    using Serilog.Context;

    public class LoggingMiddleware : OwinMiddleware
    {
        public const string RequestIdPropertyName = "HttpRequestId";
        private readonly ILogger log = Log.ForContext<LoggingMiddleware>();

        public LoggingMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var requestId = context.Environment["owin.RequestId"];
            var requestUrl = context.Request.Uri;
            using (LogContext.PushProperty(RequestIdPropertyName, requestId))
            {
                log.Information("Begin HTTP request {Url}", requestUrl);
                await Next.Invoke(context);
                log.Information("End HTTP request {Url}", requestUrl);
            }
        }
    }
}