using Elect.Core.ConfigUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit;

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
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            var destinationEndpoint = Configuration.GetValueByEnv<string>("DestinationEndpoint");

            app.RunProxy(context => context
                .ForwardTo(destinationEndpoint)
                .AddXForwardedHeaders()
                .Send());
        }
    }
}