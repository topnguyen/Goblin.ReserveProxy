using System.Text;
using System.Web;
using Elect.Core.ConfigUtils;
using Elect.Logger.Logging;
using Elect.Logger.Logging.Models;
using Elect.Web.HttpUtils;
using Elect.Web.Middlewares.CorsMiddleware;
using Elect.Web.Middlewares.HttpContextMiddleware;
using Elect.Web.Middlewares.MeasureProcessingTimeMiddleware;
using Elect.Web.Middlewares.ServerInfoMiddleware;
using Elect.Web.Models;
using Goblin.ReserveProxy.GoblinProxyMiddleware;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        private readonly ProxyAuthenticationModel _proxyAuthenticationModel;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _destinationEndpoint = configuration.GetValueByEnv<string>("DestinationEndpoint");

            _proxyAuthenticationModel = configuration.GetSection<ProxyAuthenticationModel>("ProxyAuthentication");
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

            // Enable Cookie
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

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
            app.UseMiddleware<GoblinProxyAuthMiddleware>();

            // Proxy - Forward
            app.RunProxy(async context =>
            {
                var response = await context
                    .ForwardTo(_destinationEndpoint)
                    .CopyXForwardedHeaders()
                    .AddXForwardedHeaders()
                    .Send();

                if (context.Request.IsRequestFor("favicon.ico"))
                {
                    return response;
                }

                var accessTokenCookieValue = HttpUtility.UrlEncode($"Bearer {_proxyAuthenticationModel.AccessToken}");
                
                // Check existing cookie value is correct, then no need to set cookie again
                
                if (context.Request.Cookies.TryGetValue(HeaderKey.Authorization, out var existingCookieValue))
                {
                    if (existingCookieValue == accessTokenCookieValue)
                    {
                        return response;
                    }
                }
                
                // Set cookie to improve UX for next time request
                
                var cookieBuilder = new StringBuilder();

                
                cookieBuilder.Append($"{HttpUtility.UrlEncode(HeaderKey.Authorization)}={accessTokenCookieValue}"); 
                   
                cookieBuilder.Append("; HttpOnly");

                var cookie = cookieBuilder.ToString();
                    
                response.Headers.Add("Set-Cookie", cookie);

                return response;
            });
        }
    }
}