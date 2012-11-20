<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Test_Judge_Web.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        problem id:<asp:TextBox ID="pid" runat="server"></asp:TextBox>
    
        <asp:Button ID="Button1" runat="server" onclick="Button1_Click" 
            style="width: 42px" Text="Test" />
        <asp:ListBox ID="Log" runat="server" Height="415px" Width="388px"></asp:ListBox>
        <asp:TextBox ID="codeBox" runat="server" Height="413px" style="margin-top: 0px" 
            TextMode="MultiLine" Width="220px"></asp:TextBox>
    
    </div>
    </form>
</body>
</html>
