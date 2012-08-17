<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Default.aspx.cs" Inherits="Twitter_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    Consumer key:
    <asp:TextBox ID="txtConsumerKey" runat="server" /><br />
    Consumer secret:
    <asp:TextBox ID="txtConsumerSecret" runat="server" />
    <br />
    <br />
    <asp:Button ID="btnGenerateAuthorizeLink" runat="server" Text="Generate Auth Link" OnClick="btnGenerateAuthorizeLink_Click" />
</asp:Content>
