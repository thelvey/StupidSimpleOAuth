using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SimpleOAuth.OAuth;

public partial class Twitter_Default : System.Web.UI.Page
{
    protected enum OAuthProviders { Twitter, LinkedIn };
    protected enum RequestLevelModes { AtRequestToken, AtUserAuthPage };
    protected interface IProviderConfig
    {
        string RequestTokenUrl { get; }
        string UserAuthUrl { get; }
        string AccessTokenUrl { get; }
        string MethodUrl { get; }
        string DemoMethodUrl { get; }
        string RequestLevel { get; }
        List<KeyValuePair<string, string>> DemoMethodArguments { get; }
        RequestLevelModes RequestLevelMode { get; }
        List<string> TransposeResults(dynamic jsonResult);
    }
    protected class LinkedInConfig : IProviderConfig
    {
        private string baseUrl = "https://api.linkedin.com";
        
        public string RequestTokenUrl
        {
            get { return baseUrl + "/uas/oauth/requestToken"; }
        }

        public string UserAuthUrl
        {
            get { return baseUrl + "/uas/oauth/authenticate"; }
        }

        public string AccessTokenUrl
        {
            get { return baseUrl + "/uas/oauth/accessToken"; }
        }

        public string MethodUrl
        {
            get { return baseUrl + "/v1"; }
        }

        public string DemoMethodUrl
        {
            get { return "http://api.linkedin.com/v1/people/~/connections"; }
        }

        public string RequestLevel
        {
            get { return "scope=r_network"; }
        }
        public RequestLevelModes RequestLevelMode
        {
            get { return RequestLevelModes.AtRequestToken; }
        }
        public List<KeyValuePair<string, string>> DemoMethodArguments
        {
            get
            {
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

                result.Add(new KeyValuePair<string, string>("format", "json"));

                return result;
            }
        }
        public List<string> TransposeResults(dynamic jsonResult)
        {
            List<string> transposedResult = new List<string>();
            List<dynamic> results = jsonResult.values as List<dynamic>;

            foreach (dynamic d in results)
            {
                transposedResult.Add(d.firstName + " " + d.lastName);
            }
            return transposedResult;
        }
    }
    protected class TwitterConfig : IProviderConfig
    {
        public string  RequestTokenUrl
        {
            get { return "https://api.twitter.com/oauth/request_token"; }
        }

        public string UserAuthUrl
        {
            get { return "https://api.twitter.com/oauth/authorize"; }
        }

        public string AccessTokenUrl
        {
            get { return "https://api.twitter.com/oauth/access_token"; }
        }

        public string MethodUrl
        {
            get { return "https://api.twitter.com/1"; }
        }

        public string DemoMethodUrl
        {
            get { return MethodUrl + "/statuses/user_timeline.json"; }
        }
        public string RequestLevel
        {
            get { return "permission=read"; }
        }
        public RequestLevelModes RequestLevelMode
        {
            get { return RequestLevelModes.AtUserAuthPage; }
        }
        public List<KeyValuePair<string, string>> DemoMethodArguments
        {
            get { return null; }
        }
        public List<string> TransposeResults(dynamic jsonResult)
        {
            List<string> transposedResult = new List<string>();
            dynamic[] results = jsonResult as dynamic[];

            foreach (dynamic d in results)
            {
                transposedResult.Add(d.text);
            }
            return transposedResult;
        }
    }
    protected IProviderConfig PageProviderConfig
    {
        get
        {
            IProviderConfig result = null;
            
            switch (PageProvider)
            {
                case OAuthProviders.LinkedIn:
                    result = new LinkedInConfig();
                    break;
                case OAuthProviders.Twitter:
                    result = new TwitterConfig();
                    break;
            }

            return result;
        }
    }
    protected OAuthProviders PageProvider
    {
        get
        {
            OAuthProviders result = OAuthProviders.Twitter;

            if (!String.IsNullOrEmpty(Request["provider"]))
            {
                if (Request["provider"] == "linkedin")
                {
                    result = OAuthProviders.LinkedIn;
                }
            }

            return result;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!String.IsNullOrEmpty(Request["oauth_token"]))
        {
            pnlCallback.Visible = true;
            txtConsumerKey.Text = GetSessionValue("ConsumerKey");
            txtConsumerSecret.Text = GetSessionValue("ConsumerSecret");
            ltlTokenSecretCallback.Text = GetSessionValue("OAuthTokenSecret");
            ltlOAuthToken.Text = Request["oauth_token"];
            ltlOAuthVerifier.Text = Request["oauth_verifier"];
        }
    }
    protected void btnGenerateAuthorizeLink_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(txtConsumerKey.Text)) return;
        if (String.IsNullOrEmpty(txtConsumerSecret.Text)) return;
        
        OAuthConsumerConfig config = new OAuthConsumerConfig();
        config.ConsumerKey = txtConsumerKey.Text;
        config.ConsumerSecret = txtConsumerSecret.Text;

        List<KeyValuePair<string, string>> authArgs = null;

        if (PageProviderConfig.RequestLevelMode == RequestLevelModes.AtRequestToken)
        {
            string[] args = PageProviderConfig.RequestLevel.Split('=');

            authArgs = new List<KeyValuePair<string, string>>();
            authArgs.Add(new KeyValuePair<string, string>(args[0], args[1]));
        }

        AuthRequestResult authRequest = OAuthClient.GenerateUnauthorizedRequestToken(config, PageProviderConfig.RequestTokenUrl, PageProviderConfig.UserAuthUrl, authArgs);

        SetSessionValue("OAuthTokenSecret", authRequest.OAuthTokenSecret);
        SetSessionValue("ConsumerKey", txtConsumerKey.Text);
        SetSessionValue("ConsumerSecret", txtConsumerSecret.Text);

        lnkAuth.NavigateUrl = authRequest.AuthUrl + (PageProviderConfig.RequestLevelMode == RequestLevelModes.AtUserAuthPage ? "&" + PageProviderConfig.RequestLevel : String.Empty);
        ltlTokenSecret.Text = authRequest.OAuthTokenSecret;
        pnlAuthLink.Visible = true;
    }
    protected void btnExchange_Click(object sender, EventArgs e)
    {
        OAuthConsumerConfig config = new OAuthConsumerConfig();
        config.ConsumerKey = txtConsumerKey.Text;
        config.ConsumerSecret = txtConsumerSecret.Text;

        AccessToken accessToken = OAuthClient.ExchangeForAccessToken(config, PageProviderConfig.AccessTokenUrl, ltlOAuthToken.Text, ltlTokenSecretCallback.Text, ltlOAuthVerifier.Text);

        pnlAccessToken.Visible = true;
        ltlAccessToken.Text = accessToken.OAuthToken;
        ltlAccessTokenSecret.Text = accessToken.OAuthTokenSecret;
    }
    protected void btnGetTweets_Click(object sender, EventArgs e)
    {
        OAuthConsumerConfig config = new OAuthConsumerConfig();
        config.ConsumerKey = txtConsumerKey.Text;
        config.ConsumerSecret = txtConsumerSecret.Text;

        dynamic tweets = OAuthClient.JsonMethod(config, PageProviderConfig.DemoMethodUrl, ltlAccessToken.Text, ltlAccessTokenSecret.Text, PageProviderConfig.DemoMethodArguments);

        grdTweets.DataSource = PageProviderConfig.TransposeResults(tweets);
        grdTweets.DataBind();
    }
    protected void btnReset_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Twitter/Default.aspx");
    }

    private string GetSessionValue(string valueName)
    {
        return Session[PageProvider.ToString() + '.' + valueName].ToString();
    }
    private void SetSessionValue(string valueName, object value)
    {
        Session[PageProvider.ToString() + '.' + valueName] = value;
    }
}