using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace SimpleOAuth.OAuth
{
    public interface IOAuthHelpers
    {
        string UrlEncode(string input);
        string BuildBaseString(string verb, string url, List<KeyValuePair<string, string>> parameters);
        string BuildTimestamp();
        string BuildNonce();
        byte[] GetBytes(string input);
        Dictionary<string, string> SplitResponseParams(string response);
    }
    public interface IOAuthRequest
    {
        string BuildAndExecuteRequest(string url, string consumerSecret, List<KeyValuePair<string, string>> requestParams, bool useAuthorized = true);
    }
    public interface IHttpImplemenation
    {
        HttpWebRequest CreateRequest(string url);
    }
    internal class HttpImplementation : IHttpImplemenation
    {
        public HttpWebRequest CreateRequest(string url)
        {
            return (HttpWebRequest)WebRequest.Create(url);
        }
    }
    internal class OAuthRequest : IOAuthRequest
    {
        private IOAuthHelpers _helpers = new Helpers();
        internal void SetHelperImplementation(IOAuthHelpers helper)
        {
            _helpers = helper;
        }

        private IHttpImplemenation _httpImplemenation = new HttpImplementation();
        internal void SetHttpImplementaton(IHttpImplemenation http)
        {
            _httpImplemenation = http;
        }

        public string BuildAndExecuteRequest(string url, string consumerSecret, List<KeyValuePair<string, string>> requestParams, bool useAuthorized = true)
        {
            string result = String.Empty;

            if (useAuthorized)
            {
                string baseString = _helpers.BuildBaseString("GET", url, requestParams);

                HMACSHA1 hash = new HMACSHA1();
                hash.Key = _helpers.GetBytes(consumerSecret);
                string sig = _helpers.UrlEncode(Convert.ToBase64String(hash.ComputeHash(_helpers.GetBytes(baseString))));
                requestParams.Add(new KeyValuePair<string, string>("oauth_signature", sig));
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
                HttpWebRequest r = _httpImplemenation.CreateRequest(requestUrl);

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
    }
    internal class Helpers : IOAuthHelpers
    {
        /// <summary>
        /// Url Encode based on OAuth spec
        /// HttpUtility.UrlEncode uses lower case letters for the encoded hex values
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string UrlEncode(string input)
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
        public string BuildBaseString(string verb, string url, List<KeyValuePair<string, string>> parameters)
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
        public string BuildTimestamp()
        {
            return ((int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }
        /// <summary>
        /// Generate a nonce value to be used in the signature
        /// The value doesn't really matter as long as it's unique
        /// </summary>
        /// <returns>A string containing a nonce value</returns>
        public string BuildNonce()
        {
            return DateTime.Now.Millisecond.ToString() + Guid.NewGuid().ToString().Replace("-", String.Empty);
        }


        public byte[] GetBytes(string input)
        {
            return System.Text.Encoding.ASCII.GetBytes(input);
        }
        public Dictionary<string, string> SplitResponseParams(string response)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] p = response.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string par in p)
            {
                string[] x = par.Split('=');

                result.Add(x[0], x[1]);
            }
            return result;
        }
    }
}
