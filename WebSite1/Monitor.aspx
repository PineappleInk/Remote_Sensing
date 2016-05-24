<%@ Page Language="C#" %>

<!DOCTYPE html>

<script runat="server">

    protected void Button1_Click(object sender, EventArgs e)
    {
        Response.Redirect(Request.RawUrl);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        #form1 {
            width: 1230px;
        }
    </style>
</head>
<body style="width: 1359px">
    <asp:Image runat="server" Stretch="UniformToFill" Margin="504.8,10.6,6.2,310.8" Width="1357px" Height="79px" ImageUrl="~/images.png" style="text-align: center" ImageAlign="Middle"/>
    <form id="form1" runat="server">
        <asp:Image ID="Image1" runat="server" Height="51px" ImageAlign="AbsBottom" ImageUrl="~/pineapple ink-logo-02.png" Width="38px" />
        <asp:TextBox ID="TextBox1" runat="server" Height="37px" Width="151px" BorderStyle="None" Font-Names="Segoe UI" Font-Size="Large" ForeColor="#FFCC00" Wrap="False">Pineapple inc.</asp:TextBox>  
        <asp:Image runat="server" Stretch="UniformToFill" Margin="504.8,10.6,6.2,310.8" Width="472px" Height="277px" ImageUrl="~/Logo.png" style="text-align: center" ImageAlign="Right"/>
    <div style="padding-left:350px; text-align: left; width: 877px;">
        <asp:TextBox ID="TextBox2" runat="server" BorderStyle="None" Font-Names="Segoe UI" Font-Size="Large" ForeColor="#3366FF" Height="36px" >Baby monitoring system</asp:TextBox>
        <br />
        <br />
        <asp:Xml ID="Xml1" runat="server" DocumentSource="~/XMLFile.xml" TransformSource="~/XSLTFile.xslt"></asp:Xml>
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Refresh" />
        </div>  
        <br />
        <br />
        <p>
            &nbsp;</p>
    </form>
    </body>
</html>
