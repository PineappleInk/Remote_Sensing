<%@ Page Language="C#" %>

<!DOCTYPE html>

<script runat="server">


    protected void Page_Load(object sender, EventArgs e)
    {
        Xml1.DocumentSource = @"C:\Users\Jakob\Documents\Skola\Kandidatprojekt\Remote_Sensing\WebSite1\XMLFile.xml";
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        Response.Redirect(Request.RawUrl);
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        Memberspage<br />
        <br />
        </div>
        <asp:Xml ID="Xml1" runat="server" DocumentSource="~/XMLFile.xml" OnLoad="Page_Load"></asp:Xml>
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Refresh" />
    </form>
    </body>
</html>
