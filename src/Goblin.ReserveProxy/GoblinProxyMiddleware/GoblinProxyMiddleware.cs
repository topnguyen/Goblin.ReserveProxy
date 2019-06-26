using System;
using System.Threading.Tasks;
using Elect.Core.ConfigUtils;
using Elect.Web.Models;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Goblin.ReserveProxy.GoblinProxyMiddleware
{
    public class GoblinProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ProxyAuthenticationModel _proxyAuthenticationModel;


        public GoblinProxyMiddleware(RequestDelegate next, IConfiguration configuration)
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

            var isValidAuthByAccessToken = AccessTokenHelper.IsValidAuth(context, _proxyAuthenticationModel);

            if (isValidAuthByAccessToken)
            {
                await _next.Invoke(context).ConfigureAwait(true);
                
                context.Response.Cookies.Append(HeaderKey.Authorization, $"Bearer {_proxyAuthenticationModel.AccessToken}", new CookieOptions
                {
                    Path = "/",
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.Now.AddMonths(1),
                    HttpOnly = true,
                    Secure = false 
                });

                return;
            }

            var isValidAuthByBasicAuth = BasicAuthHelper.IsValidAuth(context, _proxyAuthenticationModel);

            if (isValidAuthByBasicAuth)
            {
                await _next.Invoke(context).ConfigureAwait(true);
                
                context.Response.Cookies.Append(HeaderKey.Authorization, $"Basic {_proxyAuthenticationModel.UserName}:{_proxyAuthenticationModel.Password}", new CookieOptions
                {
                    Path = "/",
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.Now.AddMonths(1),
                    HttpOnly = true,
                    Secure = false 
                });

                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync(string.Empty).ConfigureAwait(true);
        }
    }
}