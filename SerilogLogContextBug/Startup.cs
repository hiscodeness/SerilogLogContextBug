namespace SerilogLogContextBug
{
    using Microsoft.Owin.Security.DataHandler.Encoder;
    using Microsoft.Owin.Security.Jwt;
    using Owin;
    using Serilog;

    /// <summary>
    /// Run the example and look at debug output to see the problem. LogContext HttpRequestId is set
    /// in LoggingMiddleware, but it is not output when logging in the PingModule controller:
    /// <SerilogLogContextBug.LoggingMiddleware> [80000149-0007-fd00-b63f-84710c7967bb] Begin HTTP request http://localhost:63141/
    /// <SerilogLogContextBug.PingModule>        [] Pinged!
    /// <SerilogLogContextBug.LoggingMiddleware> [80000149-0007-fd00-b63f-84710c7967bb] End HTTP request http://localhost:63141/
    /// 
    /// What it should show is the same request ID also in the ping module. By removing app.UseJwtBearerAuthentication(...) in
    /// the Configuration(IAppBuilder app) method, you get the correct logging output:
    /// <SerilogLogContextBug.LoggingMiddleware> [80000a8a-0006-fd00-b63f-84710c7967bb] Begin HTTP request http://localhost:63141/
    /// <SerilogLogContextBug.PingModule>        [80000a8a-0006-fd00-b63f-84710c7967bb] Pinged!
    /// <SerilogLogContextBug.LoggingMiddleware> [80000a8a-0006-fd00-b63f-84710c7967bb] End HTTP request http://localhost:63141/
    /// </summary>
    public class Startup
    {
        public Startup()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Trace(
                    outputTemplate:
                        "{Timestamp:u} <{SourceContext}> [{" +
                        LoggingMiddleware.RequestIdPropertyName +
                        "}] {Message}{NewLine}{Exception}")
                .CreateLogger();
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use<LoggingMiddleware>();
            // Commenting the next line will fix the issue.
            app.UseJwtBearerAuthentication(CreateJwtBearerAuthOptions());
            app.UseNancy();
        }

        private JwtBearerAuthenticationOptions CreateJwtBearerAuthOptions()
        {
            return new JwtBearerAuthenticationOptions
            {
                AllowedAudiences = new[] {"Foo"},
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                {
                    new SymmetricKeyIssuerSecurityTokenProvider("Bar", TextEncodings.Base64Url.Decode("Foobar"))
                }
            };
        }
    }
}