using Elect.Core.ConfigUtils;
using Elect.Logger.Logging;
using Elect.Logger.Logging.Models;
using Elect.Web.Middlewares.CorsMiddleware;
using Elect.Web.Middlewares.HttpContextMiddleware;
using Elect.Web.Middlewares.MeasureProcessingTimeMiddleware;
using Elect.Web.Middlewares.ServerInfoMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProxyKit;

namespace Goblin.ReserveProxy
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        private readonly string _destinationEndpoint;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            _destinationEndpoint = configuration.GetValueByEnv<string>("DestinationEndpoint");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Logger
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var electLogOptions = Configuration.GetSection<ElectLogOptions>("ElectLog");
            services.AddElectLog(electLogOptions);

            // Http Context
            services.AddElectHttpContext();

            // Server Info
            services.AddElectServerInfo();
            
            // Cors
            services.AddElectCors();

            // Proxy
            services.AddProxy();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Log
            app.UseElectLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Cors
            app.UseElectCors();

            // Http Context
            app.UseElectHttpContext();

            // Service Measure
            app.UseElectMeasureProcessingTime();

            // Server Info
            app.UseElectServerInfo();

            // Proxy - Authentication
            app.UseMiddleware<GoblinProxyMiddleware>();

            // Proxy - Forward

            app.RunProxy(context => context
                .ForwardTo(_destinationEndpoint)
                .CopyXForwardedHeaders()
                .AddXForwardedHeaders()
                .Send());
        }
    }
}