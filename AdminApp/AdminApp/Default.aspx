<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AdminApp._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %></h1>
            </hgroup>
            <p>
                This is the home page of the Face Matching Application
            </p>
            <p>
                The administrator of the sistem can handle the application database in a fast and easy way
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h3>The administrator is suggested to perform the following actions:</h3>
    <ol class="round">
        <li class="one">
            <h5>Log in</h5>
            The administrator must insert his credentials in order to access the database driven web page
        </li>
        <li class="two">
            <h5>Handle the database</h5>
            The administrator is able to manage the application database: : he can create, read, update and delete users
        </li>
        <li class="three">
            <h5>Verify the status of the system</h5>
            The administrator can verify that the system works properly, according to the attributes of the registered users
        </li>
    </ol>
</asp:Content>
