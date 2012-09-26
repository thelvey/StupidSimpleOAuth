using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SimpleOAuth.OAuth;

public partial class Implementations : System.Web.UI.Page
{
    protected IProviderConfig PageProviderConfig
    {
        get
        {
            return ProviderConfigFactory.GetImplementation(PageProvider);
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