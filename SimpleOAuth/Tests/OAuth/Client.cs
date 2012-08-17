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
    }
}
