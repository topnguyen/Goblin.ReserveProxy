using System.Linq;
using Elect.Web.Models;
using Goblin.ReserveProxy.Models;
using Microsoft.AspNetCore.Http;

namespace Goblin.ReserveProxy.Auth
{
    public static class BasicAuthHelper
    {
        /// <summary>
        ///     Check authentication by Basic Authentication (Header then Query)
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
            if (!context.Request.Cookies.TryGetValue(HeaderKey.Authorization, out var basicAuth))
            {
                return false;
            }
            
            basicAuth = basicAuth.Replace("+", " ");
            
            var basicAuthParams = basicAuth?.Trim().Split(" ").LastOrDefault()?.Split(":").ToList();

            if (basicAuthParams?.Count != 2)
            {
                return false;
            }

            var paramUserName = basicAuthParams.First();
                
            var paramPassword = basicAuthParams.Last();

            return paramUserName == proxyAuthenticationModel.UserName && paramPassword == proxyAuthenticationModel.Password;

        }
        
        private static bool IsValidAuthInHeader(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            if (!context.Request.Headers.TryGetValue(HeaderKey.Authorization, out var basicAuth))
            {
                return false;
            }
            
            var basicAuthParams = basicAuth.ToString().Trim().Split(" ").LastOrDefault()?.Split(":").ToList();

            if (basicAuthParams?.Count != 2)
            {
                return false;
            }

            var paramUserName = basicAuthParams.First();
                
            var paramPassword = basicAuthParams.Last();

            return paramUserName == proxyAuthenticationModel.UserName && paramPassword == proxyAuthenticationModel.Password;
        }
        
        private static bool IsValidAuthInQuery(HttpContext context, ProxyAuthenticationModel proxyAuthenticationModel)
        {
            context.Request.Query.TryGetValue("username", out var paramUserName);
                
            context.Request.Query.TryGetValue("password", out var paramPassword);
            
            return paramUserName == proxyAuthenticationModel.UserName && paramPassword == proxyAuthenticationModel.Password;
        }
    }
}