using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Twitter_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnGenerateAuthorizeLink_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(txtConsumerKey.Text)) return;
        if (String.IsNullOrEmpty(txtConsumerSecret.Text)) return;


    }
}