<%@ Import Namespace="Sitecore.Analytics"%>
<%@ Import Namespace="Sitecore.Analytics.Model"%>
<%@ Import Namespace="Sitecore.Analytics.Tracking"%>
<%@ Import Namespace="System.Web.Script.Serialization"%>
<%@ Import Namespace="Sitecore.Web" %>
<%@ Import Namespace="Sitecore.Analytics.Data" %>
<%@ Import Namespace="Sitecore.Commerce.Automation.MarketingAutomation" %>
<%@ Import Namespace="Sitecore.Analytics.Automation.MarketingAutomation"%>
<%@ Page Language="c#" Inherits="System.Web.UI.Page" CodePage="65001" Debug="true"%>
<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.WebControls" Assembly="Sitecore.Kernel" %>
<%@ OutputCache Location="None" VaryByParam="none" %>
<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">

<script runat="server">
	public class MergedObj {
		public UrlData url;
		public string name;
		public string data;
		public string text;
		public int value;
		public IDictionary<string, object> custom;
	}
  public void WriteTestResults()
  {
	var pages = ((CurrentVisitContext)Tracker.Current.Interaction).GetPages();

	Response.Write("<div id=\"PageEvents\">");

	foreach (var p in pages) 
	{
		var temp="";
		
		temp += "<div id='" + p.Url + "'>";
		temp += "Page Guid=" + p.Item.Id + "<BR>";
		temp += "Page Url=" + p.Url + "<BR>";
		
		bool writePage = false;

		int i=0;
		foreach (var pe in p.PageEvents)
		{
			writePage = true;

			object custom = null;
			if(pe.CustomValues.ContainsKey("Custom"))
			{
				custom = pe.CustomValues["Custom"];
			}

			var obj = new MergedObj { url=p.Url, name=pe.Name, data=pe.Data, text=pe.Text, value=pe.Value, custom=pe.CustomValues};
			
			var json = new JavaScriptSerializer().Serialize(obj);
			temp+="<textarea id='event" + i + "' cols='160' rows='5'>" + json + "</textarea><BR>";
			temp+="<textarea id='customevt" + i + "' cols='160' rows='5'>" + custom + "</textarea><BR><BR><BR>";
			i++;
		}
		
		temp += "</div>";
		if (writePage) Response.Write(temp);
	}

	Response.Write("</div>");
  }
</script>

<head runat="server">
  <title>PageEvents</title>
  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta name="CODE_LANGUAGE" content="C#" />
  <meta name="vs_defaultClientScript" content="JavaScript" />
  <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
  <link href="/default.css" rel="stylesheet" />
  <sc:VisitorIdentification runat="server" />
</head>
<body> 
  <form id="mainform" method="post" runat="server">
    <div id="MainPanel">
      <!-- Traffic type will be written here: -->
	  <div id="TestResults">
       <asp:Label style="color:Navy" ID="Result" runat="server"><% WriteTestResults(); %></asp:Label>
	  </div>
      <sc:placeholder key="main" runat="server" /> 
    </div>
  </form>
 </body>
</html>
