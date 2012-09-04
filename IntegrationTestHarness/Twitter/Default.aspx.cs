using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SimpleOAuth.OAuth;
using SimpleOAuth.Implementations;

public partial class Twitter_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!String.IsNullOrEmpty(Request["oauth_token"]))
        {
            pnlCallback.Visible = true;
            txtConsumerKey.Text = Session["Twitter.ConsumerKey"].ToString();
            txtConsumerSecret.Text = Session["Twitter.ConsumerSecret"].ToString();
            ltlTokenSecretCallback.Text = Session["Twitter.OAuthTokenSecret"].ToString();
            ltlOAuthToken.Text = Request["oauth_token"];
            ltlOAuthVerifier.Text = Request["oauth_verifier"];
        }
    }
    protected void btnGenerateAuthorizeLink_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(txtConsumerKey.Text)) return;
        if (String.IsNullOrEmpty(txtConsumerSecret.Text)) return;

        AuthRequestResult authRequest = Twitter.GenerateUnauthorizedRequestToken(txtConsumerKey.Text, txtConsumerSecret.Text);

        Session["Twitter.OAuthTokenSecret"] = authRequest.OAuthTokenSecret;
        Session["Twitter.ConsumerKey"] = txtConsumerKey.Text;
        Session["Twitter.ConsumerSecret"] = txtConsumerSecret.Text;

        lnkAuth.NavigateUrl = authRequest.AuthUrl;
        ltlTokenSecret.Text = authRequest.OAuthTokenSecret;
        pnlAuthLink.Visible = true;
    }
    protected void btnExchange_Click(object sender, EventArgs e)
    {
        OAuthConsumerConfig config = new OAuthConsumerConfig();
        config.ConsumerKey = txtConsumerKey.Text;
        config.ConsumerSecret = txtConsumerSecret.Text;

        AccessToken accessToken = Twitter.ExchangeForAccessToken(config, ltlOAuthToken.Text, ltlTokenSecretCallback.Text, ltlOAuthVerifier.Text);

        pnlAccessToken.Visible = true;
        ltlAccessToken.Text = accessToken.OAuthToken;
        ltlAccessTokenSecret.Text = accessToken.OAuthTokenSecret;
    }
    protected void btnGetTweets_Click(object sender, EventArgs e)
    {
        OAuthConsumerConfig config = new OAuthConsumerConfig();
        config.ConsumerKey = txtConsumerKey.Text;
        config.ConsumerSecret = txtConsumerSecret.Text;

        dynamic tweets = Twitter.GetTweets(config, ltlAccessToken.Text, ltlAccessTokenSecret.Text);

        grdTweets.DataSource = tweets as object[];
        grdTweets.DataBind();
    }
    protected void btnReset_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Twitter/Default.aspx");
    }
}