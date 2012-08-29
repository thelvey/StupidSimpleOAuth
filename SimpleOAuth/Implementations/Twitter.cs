using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleOAuth.OAuth;

namespace SimpleOAuth.Implementations
{
    public static class Twitter
    {
        private const string _requestTokenUrl = "https://api.twitter.com/oauth/request_token";
        private const string _userAuthUrl = "https://api.twitter.com/oauth/authorize";
        private const string _accessTokenUrl = "https://api.twitter.com/oauth/access_token";
        private const string _methodUrl = "https://api.twitter.com/1";

        private static OAuthClient _oAuthClient = new OAuthClient();

        public static AuthRequestResult GenerateUnauthorizedRequestToken(string consumerKey, string consumerSecret)
        {
            _oAuthClient.ConsumerKey = consumerKey;
            _oAuthClient.ConsumerSecret = consumerSecret;

            return _oAuthClient.GenerateUnauthorizedRequestToken(_requestTokenUrl, _userAuthUrl);
        }
        public static AccessToken ExchangeForAccessToken(string authToken, string tokenSecret, string verifier)
        {
            return _oAuthClient.ExchangeForAccessToken(_accessTokenUrl, authToken, tokenSecret, verifier);
        }
        public static dynamic GetTweets(string authToken, string tokenSecret)
        {
            return _oAuthClient.JsonMethod(_methodUrl + "/statuses/user_timeline.json", authToken, tokenSecret);
        }
    }
}
