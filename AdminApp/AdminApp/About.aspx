<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="AdminApp.About" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p style="font-size:120%;">        
            This application has been designed by Lorenzo Chelini and Christian Palmiero for the course "Projects and laboratory on communication systems",
            taught by Prof. Albertengo and Prof. Casetti at the Polytechnic University of Turin
        </p>
        <img src="polito_logo.png" alt="Polito" align="middle" height="250" width="250"/>
    </article>
</asp:Content>