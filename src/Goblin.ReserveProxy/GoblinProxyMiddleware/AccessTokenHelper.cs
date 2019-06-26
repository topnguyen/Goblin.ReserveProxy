using Elect.Web.Models;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Http;

namespace Goblin.ReserveProxy.GoblinProxyMiddleware
{
    public static class AccessTokenHelper
    {

        /// <summary>
        ///     Check authentication by Access Token (Header then Query)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="proxyAuthenticationModel"></param>
        /// <returns></returns>
        public static bool IsValidAuth(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            var isValidAuthInCookie = IsValidAuthInCookie(context, proxyAuthenticationModel);
            
            var isValidAuthInHeader = IsValidAuthInHeader(context, proxyAuthenticationModel);
            
            var isValidAuthInQuery = IsValidAuthInQuery(context, proxyAuthenticationModel);

            var isValidAuth = isValidAuthInCookie || isValidAuthInHeader || isValidAuthInQuery;
            
            return isValidAuth;
        }

        private static bool IsValidAuthInCookie(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            if (context.Request.Cookies.TryGetValue(HeaderKey.Authorization, out var accessToken))
            {
                return accessToken?.Trim() == proxyAuthenticationModel.AccessToken;
            }

            return false;
        }
        
        private static bool IsValidAuthInHeader(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            if (context.Request.Headers.TryGetValue(HeaderKey.Authorization, out var accessToken))
            {
                return accessToken.ToString()?.Trim() == proxyAuthenticationModel.AccessToken;
            }

            return false;
        }
        
        private static bool IsValidAuthInQuery(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            if (context.Request.Query.TryGetValue("token", out var accessToken))
            {
                return accessToken.ToString()?.Trim() == proxyAuthenticationModel.AccessToken;
            }

            return false;
        }
    }
}