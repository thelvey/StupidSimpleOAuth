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
            AccessToken result = new AccessToken();

            string timeStamp = GetTimestamp();
            string nonce = GetNonce();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>();

            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
            requestParams.Add(new KeyValuePair<string, string>("oauth_verifier", verifier));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));

            string response = BuildAndExecuteRequest(_accessTokenUrl, ConsumerSecret + "&" + tokenSecret, requestParams);

            Dictionary<string, string> args = SplitResponseParams(response);

            if (args.ContainsKey("oauth_token")) result.OAuthToken = args["oauth_token"];
            if (args.ContainsKey("oauth_token_secret")) result.OAuthTokenSecret = args["oauth_token_secret"];

            return result;
        }


        public AuthRequestResult GenerateUnauthorizedRequestToken()
        {
            ValidateArguments();

            AuthRequestResult result = new AuthRequestResult();

            string timeStamp = GetTimestamp();
            string nonce = GetNonce();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>();
            requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
            requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
            requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
            requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
            requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));

            // at this point call the new method
            string response = BuildAndExecuteRequest(_requestTokenUrl, ConsumerSecret + "&", requestParams);
            Dictionary<string, string> args = SplitResponseParams(response);

            if (args.ContainsKey("oauth_token"))
            {
                //http://vimeo.com/oauth/authorize
                result.AuthUrl = _userAuthUrl + "?oauth_token=" + args["oauth_token"] + "&permission=read";
                result.OAuthTokenSecret = args["oauth_token_secret"];
            }

            return result;
        }
        public dynamic JsonMethod(string url, string authToken, string tokenSecret, List<KeyValuePair<string, string>> requestParams = null)
        {
            dynamic result = null;

            if (requestParams == null) requestParams = new List<KeyValuePair<string, string>>();

            bool useAuthorized = false;

            if (!String.IsNullOrEmpty(authToken) && !String.IsNullOrEmpty(tokenSecret))
            {
                useAuthorized = true;
                string timeStamp = GetTimestamp();
                string nonce = GetNonce();

                requestParams.Add(new KeyValuePair<string, string>("oauth_consumer_key", ConsumerKey));
                requestParams.Add(new KeyValuePair<string, string>("oauth_nonce", nonce));
                requestParams.Add(new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod));
                requestParams.Add(new KeyValuePair<string, string>("oauth_timestamp", timeStamp));
                requestParams.Add(new KeyValuePair<string, string>("oauth_token", authToken));
                requestParams.Add(new KeyValuePair<string, string>("oauth_version", OAuthVersion));
            }
            string response = BuildAndExecuteRequest(url, ConsumerSecret + "&" + tokenSecret, requestParams, useAuthorized);

            if (!String.IsNullOrEmpty(response))
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { new Json.DynamicJsonObject.DynamicJsonConverter() });

                try
                {
                    result = jss.Deserialize(response, typeof(object)) as dynamic;
                }
                catch (Exception exc)
                {
                    // failed to parse json string
                }
            }

            return result;
        }

        private string BuildAndExecuteRequest(string url, string consumerSecret, List<KeyValuePair<string, string>> requestParams, bool useAuthorized = true)
        {
            string result = String.Empty;

            if (useAuthorized)
            {
                string baseString = BuildBaseString(url, requestParams);

                HMACSHA1 hash = new HMACSHA1();
                hash.Key = GetBytes(consumerSecret);
                string sig = UrlEncode(Convert.ToBase64String(hash.ComputeHash(GetBytes(baseString))));
                requestParams.Add(new KeyValuePair<string, string>("oauth_signature", sig.Replace("+", "%2B")));
            }
            string requestUrl = url;

            foreach (KeyValuePair<string, string> kvp in requestParams)
            {
                if (requestUrl == url) // first one
                {
                    requestUrl += "?";
                }
                else
                {
                    requestUrl += "&";
                }
                requestUrl += kvp.Key + "=" + kvp.Value;
            }

            try
            {
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(requestUrl);

                WebResponse response = r.GetResponse();
                Stream s = response.GetResponseStream();

                StreamReader sr = new StreamReader(s);
                result = sr.ReadToEnd();
            }
            catch (Exception exc)
            {

            }

            return result;
        }

        internal static Dictionary<string, string> SplitResponseParams(string response)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] p = response.Split(new char[]{'&'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string par in p)
            {
                string[] x = par.Split('=');

                result.Add(x[0], x[1]);
            }
            return result;
        }

        internal void ValidateArguments()
        {
            if(String.IsNullOrEmpty(ConsumerKey)) throw new ArgumentNullException();
            if(String.IsNullOrEmpty(ConsumerSecret)) throw new ArgumentNullException();
        }

        private static byte[] GetBytes(string input)
        {
            return System.Text.Encoding.ASCII.GetBytes(input);
        }
        private static string GetNonce()
        {
            return DateTime.Now.Millisecond.ToString() + Guid.NewGuid().ToString().Replace("-", String.Empty);
        }
        private static string GetTimestamp()
        {
            return ((int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }
        internal static string BuildBaseString(string url, List<KeyValuePair<string, string>> parameters)
        {
            return BuildBaseString("GET", url, parameters);
        }
        internal static string BuildBaseString(string verb, string url, List<KeyValuePair<string, string>> parameters)
        {
            string result = string.Empty;

            result += verb + "&";
            result += UrlEncode(url) + "&";

            var ps = parameters.OrderBy(p => p.Key);

            string paramstring = String.Empty;

            foreach (KeyValuePair<string, string> kv in ps)
            {
                if (paramstring != String.Empty) paramstring += "&";
                paramstring += kv.Key + "=" + kv.Value;
            }
            result += UrlEncode(paramstring);

            return result;
        }
        /// <summary>
        /// Url Encode based on OAuth spec
        /// HttpUtility.UrlEncode uses lower case letters for the encoded hex values
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string UrlEncode(string input)
        {
            string result = input;

            result = result.Replace("%", "%25");
            result = result.Replace(":", "%3A");
            result = result.Replace("&", "%26");
            result = result.Replace("=", "%3D");
            result = result.Replace("+", "%2B");
            result = result.Replace("$", "%24");
            result = result.Replace("/", "%2F");
            result = result.Replace("?", "%3F");

            return result;
        }
    }
}
