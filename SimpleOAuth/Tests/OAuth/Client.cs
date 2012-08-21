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
            Dictionary<string, string> result = OAuthClient.SplitResponseParams(String.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        [Test]
        public void SplitReponseParams()
        {
            Dictionary<string, string> result = OAuthClient.SplitResponseParams("this=1&that=2");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("1", result["this"]);
            Assert.AreEqual("2", result["that"]);
        }
        [Test]
        public void UrlEncode()
        {
            string urlToEncode = "http://test.com/?key=value&text=a:b%c+d$e";

            string encoded = OAuthClient.UrlEncode(urlToEncode);

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

            string baseString = OAuthClient.BuildBaseString(verb, url, args);

            Assert.AreEqual("GET&http%3A%2F%2Fphotos.example.net%2Fphotos&file%3Dvacation.jpg%26oauth_consumer_key%3Ddpf43f3p2l4k3l03%26oauth_nonce%3Dkllo9940pd9333jh%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D1191242096%26oauth_token%3Dnnch734d00sl2jdk%26oauth_version%3D1.0%26size%3Doriginal",
                baseString);
        }
    }
}
