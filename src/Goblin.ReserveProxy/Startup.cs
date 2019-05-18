using System;
using System.Data;
using Elect.Core.ConfigUtils;
using Elect.Web.Middlewares.HttpContextMiddleware;
using Elect.Web.Middlewares.MeasureProcessingTimeMiddleware;
using Elect.Web.Middlewares.ReverseProxyMiddleware;
using Elect.Web.Middlewares.ServerInfoMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Goblin.ReserveProxy
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddElectReserveProxy(_ =>
            {
                var destinationEndpoint = Configuration.GetValueByEnv<string>("DestinationEndpoint");

                bool isValidUri = Uri.TryCreate(destinationEndpoint, UriKind.Absolute, out var destinationUri);

                if (!isValidUri)
                {
                    throw new InvalidConstraintException("Your destination domain is not valid absolute URI");
                }
                
                _.ServiceRootUrl = destinationUri.ToString();

                _.BeforeReserveProxy = context =>
                {
                    Console.WriteLine($"[Before Reserve] {context.Request.GetDisplayUrl()}");
                    return true;
                };
                
                _.AfterReserveProxy = context =>
                {
                    Console.WriteLine($"[After Reserve] {context.Request.GetDisplayUrl()}");
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseElectHttpContext();

            app.UseElectMeasureProcessingTime();

            app.UseElectServerInfo();

            app.UseElectReserveProxy();
        }
    }
}