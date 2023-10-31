<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BackOffice.Authorizer.Management.API.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%= GetTitle() %> v<%= GetVersion() %></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <%= GetApplicationDescription() %>
        </div>
    </form>
</body>
</html>
