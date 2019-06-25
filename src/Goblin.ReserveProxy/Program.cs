using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Elect.Core.Constants;
using Elect.Core.EnvUtils;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace Goblin.ReserveProxy
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            SetConsoleAppInfo();

            var webHostBuilder = CreateWebHostBuilder(args);

            var webHost = webHostBuilder.Build();

            await webHost.RunAsync();
        }

        public static void SetConsoleAppInfo()
        {
            var appInfo =
                $@"{PlatformServices.Default.Application.ApplicationName} v{PlatformServices.Default.Application.ApplicationVersion} ({EnvHelper.CurrentEnvironment})";

            Console.Title = appInfo;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(appInfo);
            Console.ResetColor();
            Console.WriteLine();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();

            if (args?.Any() == true)
            {
                var config = new ConfigurationBuilder().AddCommandLine(args).Build();

                webHostBuilder.UseConfiguration(config);
            }

            // Kestrel
            webHostBuilder.UseKestrel((context, options) =>
            {
                options.AddServerHeader = false;

                var listenUrls = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

                if (string.IsNullOrWhiteSpace(listenUrls))
                {
                    // Load Kestrel Endpoint config in app setting
                    options.Configure(context.Configuration.GetSection("Kestrel"));
                }
            });

            // Content
            var contentRoot = webHostBuilder.GetSetting(WebHostDefaults.ContentRootKey);
            if (string.IsNullOrWhiteSpace(contentRoot))
            {
                webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
            }

            // Capture Error
            webHostBuilder.CaptureStartupErrors(true);

            // DI Validate
            webHostBuilder.UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            });

            // App Config
            webHostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Delete all default configuration providers
                configBuilder.Sources.Clear();

                configBuilder.SetBasePath(Directory.GetCurrentDirectory());

                configBuilder.AddJsonFile(ConfigurationFileName.AppSettings, true, true);

                var env = context.HostingEnvironment;

                if (env.IsDevelopment())
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));

                    if (appAssembly != null)
                    {
                        configBuilder.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                configBuilder.AddEnvironmentVariables();

                if (args?.Any() == true)
                {
                    configBuilder.AddCommandLine(args);
                }
            });

            // Service Config

            webHostBuilder.ConfigureServices((context, services) =>
            {
                // Hosting Filter

                services.PostConfigure<HostFilteringOptions>(options =>
                {
                    if (options.AllowedHosts != null && options.AllowedHosts.Count != 0)
                    {
                        return;
                    }

                    var hosts = context
                        .Configuration["AllowedHosts"]?
                        .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                    options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] {"*"});
                });

                // Hosting Filter Notification

                var hostingFilterOptions =
                    new ConfigurationChangeTokenSource<HostFilteringOptions>(context.Configuration);

                services.AddSingleton<IOptionsChangeTokenSource<HostFilteringOptions>>(hostingFilterOptions);

                services.AddTransient<IStartupFilter, HostFilteringStartupFilter>();

                // IIS
                var iisConfig = context.Configuration.GetSection("IIS");

                var isUseIis = iisConfig?.GetValue("IsUseIIS", false) ?? false;

                if (isUseIis)
                {
                    webHostBuilder.UseIIS();
                }

                var isUseIisIntegration = iisConfig?.GetValue("IsUseIISIntegration", false) ?? false;

                if (isUseIisIntegration)
                {
                    webHostBuilder.UseIISIntegration();
                }
            });

            // Startup
            webHostBuilder.UseStartup(PlatformServices.Default.Application.ApplicationName);

            return webHostBuilder;
        }
    }
}