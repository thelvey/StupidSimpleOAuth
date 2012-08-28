using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SimpleOAuth.OAuth;
using Rhino.Mocks;

namespace SimpleOAuth.Tests.OAuth
{
    [TestFixture]
    public class Client
    {
        IOAuthHelpers helpers = new Helpers();

        private const string USER_AUTH_URL = "userAuthUrl";
        private const string REQUEST_TOKEN_URL = "requestTokenUrl";
        private const string ACCESS_TOKEN_URL = "accessTokenUrl";
        private const string CONSUMER_SECRET = "consumerSecret";
        private const string CONSUMER_KEY = "consumerKey";

        string timeStamp = "TIMESTAMP";
        string nonce = "NONCE";

        [Test]
        public void GenerateUnauthorizedRequestWithoutArguments()
        {
            MockRepository mr = new MockRepository();
            OAuthClient oa = new OAuthClient(String.Empty, String.Empty, String.Empty);

            using (mr.Record())
            {
                Assert.Throws(typeof(ArgumentNullException), delegate
                {
                    oa.GenerateUnauthorizedRequestToken();
                });
            }
        }
        [Test]
        public void SplitReponseParamsWithNoResponse()
        {
            Dictionary<string, string> result = helpers.SplitResponseParams(String.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        [Test]
        public void SplitReponseParams()
        {
            Dictionary<string, string> result = helpers.SplitResponseParams("this=1&that=2");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("1", result["this"]);
            Assert.AreEqual("2", result["that"]);
        }
        [Test]
        public void UrlEncode()
        {
            string urlToEncode = "http://test.com/?key=value&text=a:b%c+d$e";

            string encoded = helpers.UrlEncode(urlToEncode);

            Assert.AreEqual("http%3A%2F%2Ftest.com%2F%3Fkey%3Dvalue%26text%3Da%3Ab%25c%2Bd%24e", encoded);
        }
        /// <summary>
        /// This test uses the base string example from OAuth spec:
        /// http://oauth.net/core/1.0/#sig_base_example
        /// </summary>
        [Test]
        public void BuildBaseString()
        {
            string verb = "GET";
            string url = "http://photos.example.net/photos";

            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>();

            args.Add(new KeyValuePair<string, string>("oauth_consumer_key", "dpf43f3p2l4k3l03"));
            args.Add(new KeyValuePair<string, string>("oauth_token", "nnch734d00sl2jdk"));
            args.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
            args.Add(new KeyValuePair<string, string>("oauth_timestamp", "1191242096"));
            args.Add(new KeyValuePair<string, string>("oauth_nonce", "kllo9940pd9333jh"));
            args.Add(new KeyValuePair<string, string>("oauth_version", "1.0"));
            args.Add(new KeyValuePair<string, string>("file", "vacation.jpg"));
            args.Add(new KeyValuePair<string, string>("size", "original"));

            string baseString = helpers.BuildBaseString(verb, url, args);

            Assert.AreEqual("GET&http%3A%2F%2Fphotos.example.net%2Fphotos&file%3Dvacation.jpg%26oauth_consumer_key%3Ddpf43f3p2l4k3l03%26oauth_nonce%3Dkllo9940pd9333jh%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D1191242096%26oauth_token%3Dnnch734d00sl2jdk%26oauth_version%3D1.0%26size%3Doriginal",
                baseString);
        }
        /// <summary>
        /// It doesn't really matter what is return from this as long as it's unique,
        /// so we'll generate a bunch and make sure there are no duplicates
        /// </summary>
        [Test]
        public void GetNonce()
        {
            List<string> nonces = new List<string>();

            for(int i = 0; i < 1000; i++)
            {
                nonces.Add(helpers.BuildNonce());
            }

            var g = nonces.GroupBy(n => n);
            g.ToList().ForEach(n => Assert.AreEqual(1, n.Count()));
        }
        [Test]
        public void BuildAndExecuteRequest()
        {
            MockRepository mr = new MockRepository();

            IOAuthHelpers helpers = mr.DynamicMock<IOAuthHelpers>();
            IHttpImplemenation http = mr.DynamicMock<IHttpImplemenation>();

            OAuthRequest request = new OAuthRequest();
            request.SetHelperImplementation(helpers);
            request.SetHttpImplementaton(http);

            string testUrl = "TESTURL";
            var baseString = "BASESTRING";
            string consumerKey = "CONSUMER_KEY";
            string signature = "SIGNATURE";
            byte[] bytes = new byte[1] { (byte)1 };
            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string,string>>();
            args.Add(new KeyValuePair<string,string>("key", "value"));

            using (mr.Record())
            {
                helpers.Expect(h => h.BuildBaseString("GET", testUrl, args)).Return(baseString);
                helpers.Expect(h => h.GetBytes(consumerKey)).Return(bytes);
                helpers.Expect(h => h.GetBytes(baseString)).Return(bytes);
                helpers.Expect(h => h.UrlEncode(String.Empty)).IgnoreArguments().Return(signature);
                http.Expect(x => x.CreateRequest("TESTURL?key=value&oauth_signature=SIGNATURE")).Return(null);
            }
            using (mr.Playback())
            {
                request.BuildAndExecuteRequest(testUrl, consumerKey, args);
            }
        }

        [Test]
        public void ExchangeForAccessToken()
        {
            MockRepository m = new MockRepository();

            IOAuthHelpers _helpers = m.DynamicMock<IOAuthHelpers>();
            IOAuthRequest _request = m.DynamicMock<IOAuthRequest>();

            OAuthClient client = new OAuthClient(USER_AUTH_URL, REQUEST_TOKEN_URL, ACCESS_TOKEN_URL, CONSUMER_SECRET, CONSUMER_KEY);
            client.SetHelperImplementation(_helpers);
            client.SetRequestImplementation(_request);

            
            string tokenSecret = "tokenSecret";
            string responseToken = "responseToken";
            string responseTokenSecret = "responseTokenSecret";

            Dictionary<string, string> responseParameters = new Dictionary<string, string>();
            responseParameters.Add("oauth_token", responseToken);
            responseParameters.Add("oauth_token_secret", responseTokenSecret);

            using (m.Record())
            {
                _helpers.Expect(h => h.BuildTimestamp()).Return(timeStamp);
                _helpers.Expect(h => h.BuildNonce()).Return(nonce);

                _request.Expect(r => r.BuildAndExecuteRequest(ACCESS_TOKEN_URL, CONSUMER_SECRET + "&" + tokenSecret, null)).IgnoreArguments().Return("response");

                _helpers.Expect(h => h.SplitResponseParams(null)).IgnoreArguments().Return(responseParameters);
            }
            using (m.Playback())
            {
                AccessToken result = client.ExchangeForAccessToken("authToken", tokenSecret, "verifier");

                Assert.AreEqual(responseToken, result.OAuthToken);
                Assert.AreEqual(responseTokenSecret, result.OAuthTokenSecret);
            }
        }
        [Test]
        public void GenerateUnauthorizedRequestToken()
        {
            MockRepository m = new MockRepository();

            IOAuthHelpers helpers = m.DynamicMock<IOAuthHelpers>();
            IOAuthRequest request = m.DynamicMock<IOAuthRequest>();

            OAuthClient client = new OAuthClient(USER_AUTH_URL, REQUEST_TOKEN_URL, ACCESS_TOKEN_URL, CONSUMER_SECRET, CONSUMER_KEY);
            client.SetHelperImplementation(helpers);
            client.SetRequestImplementation(request);

            string tokenSecret = "tokenSecret";
            string responseToken = "responseToken";
            string responseTokenSecret = "responseTokenSecret";

            Dictionary<string, string> responseParameters = new Dictionary<string, string>();
            responseParameters.Add("oauth_token", responseToken);
            responseParameters.Add("oauth_token_secret", responseTokenSecret);

            using (m.Record())
            {
                helpers.Expect(h => h.BuildTimestamp()).Return(timeStamp);
                helpers.Expect(h => h.BuildNonce()).Return(nonce);

                request.Expect(r => r.BuildAndExecuteRequest(ACCESS_TOKEN_URL, CONSUMER_SECRET, null)).IgnoreArguments().Return("response");

                helpers.Expect(h => h.SplitResponseParams(null)).IgnoreArguments().Return(responseParameters);
            }
            using (m.Playback())
            {
                AuthRequestResult result = client.GenerateUnauthorizedRequestToken();

                Assert.AreEqual(USER_AUTH_URL + "?oauth_token=" + responseToken + "&permission=read", result.AuthUrl);
                Assert.AreEqual(responseTokenSecret, result.OAuthTokenSecret);
            }
        }
    }
}
