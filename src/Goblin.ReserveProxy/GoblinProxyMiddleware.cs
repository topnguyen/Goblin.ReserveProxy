using System.Linq;
using System.Threading.Tasks;
using Elect.Core.ConfigUtils;
using Elect.Web.Models;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Goblin.ReserveProxy
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
            // Check not setup any authentication on proxy
            
            if (string.IsNullOrWhiteSpace(_proxyAuthenticationModel.AccessToken) &&
                string.IsNullOrWhiteSpace(_proxyAuthenticationModel.UserName) &&
                string.IsNullOrWhiteSpace(_proxyAuthenticationModel.Password))
            {
                await _next.Invoke(context).ConfigureAwait(true);
                
                return;
            }
                        
            var isValidAuthenticationByAccessToken = IsValidAuthenticationByAccessToken(context);

            var isValidAuthenticationByBasic = IsValidAuthenticationByBasic(context);
            
            // Check Authorization

            if (!isValidAuthenticationByAccessToken && !isValidAuthenticationByBasic)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsync(string.Empty).ConfigureAwait(true);

                return;
            }

            await _next.Invoke(context).ConfigureAwait(true);
        }

        /// <summary>
        ///     Check authentication by Access Token (Header then Query)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool IsValidAuthenticationByAccessToken(HttpContext context)
        {
            var isValidAuthenticationByAccessToken = false;

            if (!context.Request.Headers.TryGetValue(HeaderKey.Authorization, out var accessToken))
            {
                if (context.Request.Query.TryGetValue("token", out accessToken))
                {
                    accessToken = accessToken.ToString().Trim();
                }
            }
            else
            {
                accessToken = accessToken.ToString().Trim().Split(" ").LastOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                isValidAuthenticationByAccessToken = accessToken == _proxyAuthenticationModel.AccessToken;
            }

            return isValidAuthenticationByAccessToken;
        }
        
        /// <summary>
        ///     Check authentication by Basic Authentication (Header then Query)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool IsValidAuthenticationByBasic(HttpContext context)
        {
            bool isValidAuthenticationByAccessToken;

            if (!context.Request.Headers.TryGetValue(HeaderKey.Authorization, out var basicAuthentication))
            {
                context.Request.Query.TryGetValue("username", out var paramUserName);
                
                context.Request.Query.TryGetValue("password", out var paramPassword);
                
                isValidAuthenticationByAccessToken = paramUserName == _proxyAuthenticationModel.UserName &&
                                                     paramPassword == _proxyAuthenticationModel.Password;
            }
            else
            {
                basicAuthentication = basicAuthentication.ToString().Trim().Split(" ").LastOrDefault();

                var paramUserName = basicAuthentication.ToString().Split(":").FirstOrDefault();
                
                var paramPassword = basicAuthentication.ToString().Split(":").LastOrDefault();

                isValidAuthenticationByAccessToken = paramUserName == _proxyAuthenticationModel.UserName &&
                                                     paramPassword == _proxyAuthenticationModel.Password;
            }

            return isValidAuthenticationByAccessToken;
        }
    }
}