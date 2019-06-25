namespace Goblin.ReserveProxy.Models
{
    public class ProxyAuthenticationModel
    {
        /// <summary>
        ///     Bearer Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        ///     Basic Authentication - UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Basic Authentication - Password
        /// </summary>
        public string Password { get; set; }
    }
}