using ExtremeSwank.OAuth;
using System.Configuration;
using System;
namespace openidrp.Code
{

    public class DossiaOauth
    {
        public static OAuthClient createDossiaOAuthClient()
        {
            OAuthClient returnValue = new OAuthClient()
            {
                ConsumerKey = ConfigurationManager.AppSettings["oauth.consumer.key"],
                ConsumerSecret = ConfigurationManager.AppSettings["oauth.consumer.secret"],
                RequestTokenUrl = new Uri(ConfigurationManager.AppSettings["oauth.request.token.url"]),
                AuthorizeTokenUrl = new Uri(ConfigurationManager.AppSettings["oauth.authorize.token.url"]),
                AccessTokenUrl = new Uri(ConfigurationManager.AppSettings["oauth.access.token.url"])
            };
            return returnValue;
        }
    }
}