<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Wiki.aspx.cs" Inherits="Test_Judge_Web.Wiki" %>
<%@ Register TagPrefix="FTB" Namespace="FreeTextBoxControls" Assembly="FreeTextBox" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:TextBox ID="txtInput" runat="server" TextMode="MultiLine" Width="100%" Rows="20"></asp:TextBox><br/>
    <asp:TextBox ID="txtOutput" runat="server" TextMode="MultiLine" Enabled="false" Width="100%" Rows="20"></asp:TextBox>
        <FTB:FreeTextBox ID="txtEditor" runat="server" ></FTB:FreeTextBox>
        <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            Text="Get Html" />
            <asp:Button ID="btnInput" runat="server" Text="Convert" 
            onclick="btnInput_Click"/>
    </div>
    </form>
</body>
</html>
