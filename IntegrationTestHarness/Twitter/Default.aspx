<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Default.aspx.cs" Inherits="Twitter_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:Button ID="btnReset" runat="server" Text="Start over" OnClick="btnReset_Click" />
    <br />
    <br />
    Consumer key:
    <asp:TextBox ID="txtConsumerKey" runat="server" Text="" /><br />
    Consumer secret:
    <asp:TextBox ID="txtConsumerSecret" runat="server" Text="" />
    <br />
    <br />
    <asp:Button ID="btnGenerateAuthorizeLink" runat="server" Text="Generate Auth Link"
        OnClick="btnGenerateAuthorizeLink_Click" />
    <br />
    <br />
    <asp:Panel ID="pnlAuthLink" runat="server" Visible="false">
        Token Secret:
        <asp:Literal ID="ltlTokenSecret" runat="server" />
        <br />
        <asp:HyperLink ID="lnkAuth" runat="server" Text="Authorize" />
        (Callback URL must be set to this page in your Twitter application settings)
        <br />
        <br />
    </asp:Panel>
    <asp:Panel ID="pnlCallback" runat="server" Visible="false">
        Token Secret:
        <asp:Literal ID="ltlTokenSecretCallback" runat="server" />
        <br />
        OAuth Token:
        <asp:Literal ID="ltlOAuthToken" runat="server" />
        <br />
        OAuth Verifier:
        <asp:Literal ID="ltlOAuthVerifier" runat="server" />
        <br />
        <asp:Button ID="btnExchange" runat="server" Text="Exchange for Access Token" OnClick="btnExchange_Click" />
        <br />
        <br />
    </asp:Panel>
    <asp:Panel ID="pnlAccessToken" runat="server" Visible="false">
        Access Token:
        <asp:Literal ID="ltlAccessToken" runat="server" />
        <br />
        Access Token Secret:
        <asp:Literal ID="ltlAccessTokenSecret" runat="server" />
        <br />
        <asp:Button ID="btnGetTweets" runat="server" Text="Get Tweets for authorized user"
            OnClick="btnGetTweets_Click" />
        <br />
        <br />
    </asp:Panel>
    <asp:GridView ID="grdTweets" runat="server" AutoGenerateColumns="false" ShowHeader="false">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <%# (Container.DataItem as dynamic).text %>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>
