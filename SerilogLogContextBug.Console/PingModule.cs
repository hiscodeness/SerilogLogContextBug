namespace SerilogLogContextBug
{
    using Nancy;
    using Serilog;

    public class PingModule : NancyModule
    {
        private readonly ILogger log = Log.ForContext<PingModule>();
        public PingModule()
        {
            Get[string.Empty] = _ =>
            {
                log.Information("Pinged!");
                return "Pong!";
            };
        }
    }
}