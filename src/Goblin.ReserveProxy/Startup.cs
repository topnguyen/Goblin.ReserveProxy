using System;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddElectReserveProxy(_ =>
            {
                _.ServiceRootUrl = "http://127.0.0.1:16686/";
                _.AfterReserveProxy = context => { Console.WriteLine(context.Request.GetDisplayUrl()); };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseElectHttpContext();

            app.UseElectMeasureProcessingTime();

            app.UseElectServerInfo();

            app.UseElectReserveProxy();
        }
    }
}