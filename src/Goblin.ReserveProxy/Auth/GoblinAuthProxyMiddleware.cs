using System.Threading.Tasks;
using Elect.Core.ConfigUtils;
using Elect.Web.HttpUtils;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Goblin.ReserveProxy.Auth
{
    public class GoblinAuthProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ProxyAuthenticationModel _proxyAuthenticationModel;


        public GoblinAuthProxyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _proxyAuthenticationModel = configuration.GetSection<ProxyAuthenticationModel>("ProxyAuthentication");
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(_proxyAuthenticationModel.AccessToken) &&
                string.IsNullOrWhiteSpace(_proxyAuthenticationModel.UserName) &&
                string.IsNullOrWhiteSpace(_proxyAuthenticationModel.Password))
            {
                await _next.Invoke(context).ConfigureAwait(true);

                return;
            }

            if (context.Request.IsRequestFor("favicon.ico"))
            {
                await _next.Invoke(context).ConfigureAwait(true);

                return;
            }

            var isValidAuthByAccessToken = AccessTokenHelper.IsValidAuth(context, _proxyAuthenticationModel);

            if (isValidAuthByAccessToken)
            {
                await _next.Invoke(context).ConfigureAwait(true);
                
                return;
            }

            var isValidAuthByBasicAuth = BasicAuthHelper.IsValidAuth(context, _proxyAuthenticationModel);

            if (isValidAuthByBasicAuth)
            {
                await _next.Invoke(context).ConfigureAwait(true);
                
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            
            await context.Response.WriteAsync(string.Empty).ConfigureAwait(true);
        }
    }
}