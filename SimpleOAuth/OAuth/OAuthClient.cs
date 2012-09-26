using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace SimpleOAuth.OAuth
{
    public class AccessToken
    {
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
    }
    public class AuthRequestResult
    {
        public string AuthUrl { get; set; }
        public string OAuthTokenSecret { get; set; }
    }
    public class OAuthProviderConfig
    {
        public string UserAuthUrl { get; set; }
        public string RequestTokenUrl { get; set; }
        public string AccessTokenUrl { get; set; }
    }
    public class OAuthConsumerConfig
    {
        public string ConsumerSecret { get; set; }
        public string ConsumerKey { get; set; }
    }
    public static class OAuthClient
    {
        private const string OAUTH_VERSION = "1.0";
        private const string SIGNATURE_METHOD = "HMAC-SHA1";

        private static IOAuthHelpers _helpers = new Helpers();
        internal static void SetHelperImplementation(IOAuthHelpers helpers)
        {
            _helpers = helpers;
        }

        private static IOAuthRequestImplementation _requestImplementation = new DefaultOAuthRequestImplementation();
        internal static void SetRequestImplementation(IOAuthRequestImplementation request)
        {
            _requestImplementation = request;
        }

        public static AccessToken ExchangeForAccessToken(OAuthConsumerConfig config, string accessTokenUrl, string authToken, string tokenSecret, string verifier)
        {
            ValidateArguments(config);

            AccessToken result = new AccessToken();

            string timeStamp = _helpers.BuildTimestamp();
            string nonce = _helpers.BuildNonce();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>();

            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", config.ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SIGNATURE_METHOD));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
            requestParams.Add(new KeyValuePair<string, string>("oauth_verifier", verifier));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));

            string response = _requestImplementation.BuildAndExecuteRequest(accessTokenUrl, config.ConsumerSecret + "&" + tokenSecret, requestParams);

            Dictionary<string, string> args = _helpers.SplitResponseParams(response);

            if (args.ContainsKey("oauth_token")) result.OAuthToken = args["oauth_token"];
            if (args.ContainsKey("oauth_token_secret")) result.OAuthTokenSecret = args["oauth_token_secret"];

            return result;
        }

        public static AuthRequestResult GenerateUnauthorizedRequestToken(OAuthConsumerConfig config, string requestTokenUrl, string userAuthUrl, List<KeyValuePair<string, string>> authArgs = null)
        {
            ValidateArguments(config);

            AuthRequestResult result = new AuthRequestResult();

            string timeStamp = _helpers.BuildTimestamp();
            string nonce = _helpers.BuildNonce();

            List<KeyValuePair<string, string>> requestParams = authArgs ?? new List<KeyValuePair<string, string>>();
            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", config.ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SIGNATURE_METHOD));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));

            string response = _requestImplementation.BuildAndExecuteRequest(requestTokenUrl, config.ConsumerSecret + "&", requestParams);
            Dictionary<string, string> args = _helpers.SplitResponseParams(response);

            if (args.ContainsKey("oauth_token"))
            {
                // the &permission is an artifact of twitter's auth
                // it isn't break other auths so i'm going to leave it here for now
                result.AuthUrl = userAuthUrl + "?oauth_token=" + args["oauth_token"];
                result.OAuthTokenSecret = args["oauth_token_secret"];
            }

            return result;
        }
        public static dynamic JsonMethod(OAuthConsumerConfig config, string url, string authToken, string tokenSecret, List<KeyValuePair<string, string>> requestParams = null)
        {
            return JsonMethod(config, url, authToken, tokenSecret, new Json.DynamicJsonObject.DynamicJsonConverter(), requestParams);
        }
        internal static dynamic JsonMethod(OAuthConsumerConfig config, string url, string authToken, string tokenSecret, JavaScriptConverter jsConverter, List<KeyValuePair<string, string>> requestParams = null)
        {
            dynamic result = null;

            if (requestParams == null) requestParams = new List<KeyValuePair<string, string>>();

            bool useAuthorized = false;

            if (!String.IsNullOrEmpty(authToken) && !String.IsNullOrEmpty(tokenSecret))
            {
                useAuthorized = true;
                string timeStamp = _helpers.BuildTimestamp();
                string nonce = _helpers.BuildNonce();

                requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", config.ConsumerKey));
                requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
                requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SIGNATURE_METHOD));
                requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
                requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
                requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAUTH_VERSION));
            }
            string response = _requestImplementation.BuildAndExecuteRequest(url, config.ConsumerSecret + "&" + tokenSecret, requestParams, useAuthorized);

            if (!String.IsNullOrEmpty(response))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { jsConverter });

                try
                {
                    result = jss.Deserialize(response, typeof(object)) as dynamic;
                }
                catch (Exception exc)
                {
                    throw new InvalidJsonInputException();
                }
            }

            return result;
        }

        internal static void ValidateArguments(OAuthConsumerConfig config)
        {
            if(String.IsNullOrEmpty(config.ConsumerKey)) throw new ArgumentNullException();
            if(String.IsNullOrEmpty(config.ConsumerSecret)) throw new ArgumentNullException();
        }
    }
}
