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
    public class OAuthClient
    {
        private string OAuthVersion = "1.0";
        private string SignatureMethod = "HMAC-SHA1";

        private readonly string _userAuthUrl;
        private readonly string _requestTokenUrl;
        private readonly string _accessTokenUrl;

        public string ConsumerSecret { get; set; }
        public string ConsumerKey { get; set; }

        private static IOAuthHelpers _helpers = new Helpers();
        internal void SetHelperImplementation(IOAuthHelpers helpers)
        {
            _helpers = helpers;
        }

        private static IOAuthRequest _requestImplementation = new OAuthRequest();
        internal void SetRequestImplementation(IOAuthRequest request)
        {
            _requestImplementation = request;
        }

        public OAuthClient(string userAuthUrl, string requestTokenUrl, string accessTokenUrl, string consumerSecret = null, string consumerKey = null)
        {
            _userAuthUrl = userAuthUrl;
            _requestTokenUrl = requestTokenUrl;
            _accessTokenUrl = accessTokenUrl;
            ConsumerSecret = consumerSecret;
            ConsumerKey = consumerKey;
        }

        public AccessToken ExchangeForAccessToken(string authToken, string tokenSecret, string verifier)
        {
            ValidateArguments();

            AccessToken result = new AccessToken();

            string timeStamp = _helpers.BuildTimestamp();
            string nonce = _helpers.BuildNonce();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>();

            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
            requestParams.Add(new KeyValuePair<string, string>("oauth_verifier", verifier));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));

            string response = _requestImplementation.BuildAndExecuteRequest(_accessTokenUrl, ConsumerSecret + "&" + tokenSecret, requestParams);

            Dictionary<string, string> args = _helpers.SplitResponseParams(response);

            if (args.ContainsKey("oauth_token")) result.OAuthToken = args["oauth_token"];
            if (args.ContainsKey("oauth_token_secret")) result.OAuthTokenSecret = args["oauth_token_secret"];

            return result;
        }


        public AuthRequestResult GenerateUnauthorizedRequestToken()
        {
            ValidateArguments();

            AuthRequestResult result = new AuthRequestResult();

            string timeStamp = _helpers.BuildTimestamp();
            string nonce = _helpers.BuildNonce();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>();
            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));

            // at this point call the new method
            string response = _requestImplementation.BuildAndExecuteRequest(_requestTokenUrl, ConsumerSecret + "&", requestParams);
            Dictionary<string, string> args = _helpers.SplitResponseParams(response);

            if (args.ContainsKey("oauth_token"))
            {
                //http://vimeo.com/oauth/authorize
                //todo: this needs changed to be made not specific to twitter
                result.AuthUrl = _userAuthUrl + "?oauth_token=" + args["oauth_token"] + "&permission=read";
                result.OAuthTokenSecret = args["oauth_token_secret"];
            }

            return result;
        }
        public dynamic JsonMethod(string url, string authToken, string tokenSecret, List<KeyValuePair<string, string>> requestParams = null)
        {
            return JsonMethod(url, authToken, tokenSecret, new Json.DynamicJsonObject.DynamicJsonConverter(), requestParams);
        }
        internal dynamic JsonMethod(string url, string authToken, string tokenSecret, JavaScriptConverter jsConverter, List<KeyValuePair<string, string>> requestParams = null)
        {
            dynamic result = null;

            if (requestParams == null) requestParams = new List<KeyValuePair<string, string>>();

            bool useAuthorized = false;

            if (!String.IsNullOrEmpty(authToken) && !String.IsNullOrEmpty(tokenSecret))
            {
                useAuthorized = true;
                string timeStamp = _helpers.BuildTimestamp();
                string nonce = _helpers.BuildNonce();

                requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
                requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
                requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
                requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
                requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
                requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));
            }
            string response = _requestImplementation.BuildAndExecuteRequest(url, ConsumerSecret + "&" + tokenSecret, requestParams, useAuthorized);

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

        internal void ValidateArguments()
        {
            if(String.IsNullOrEmpty(ConsumerKey)) throw new ArgumentNullException();
            if(String.IsNullOrEmpty(ConsumerSecret)) throw new ArgumentNullException();
        }
    }
}
