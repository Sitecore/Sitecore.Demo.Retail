<%@ Import Namespace="Sitecore.Analytics"%>
<%@ Import Namespace="Sitecore.Analytics.Model"%>
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

	var contactId = new Guid(WebUtil.GetQueryString("contact"));
	var options = new Sitecore.Analytics.DataAccess.InteractionLoadOptions(contactId, 0, 100);
    	var interactions = Sitecore.Analytics.Data.DataAccess.DataAdapterManager.Provider.LoadVisits(options);
	
	Response.Write("<h1>Mongo Page Events</h1>");

	var iCtr = 0;
	foreach (var i in interactions)
	{
		iCtr++;
		Response.Write("<div class='interaction' id='interaction" + iCtr + "'>");
		Response.Write("<H2>Interaction= " + iCtr + "</H2>");
		var pages = i.Pages;
		
		int e=0;
		foreach (var p in pages) 
		{
		    	var temp="";
			
			temp+="<div class=\"page\">";
			temp+="<b>Page Url=</b>" + p.Url + "<BR>";

			bool writePage = false;
			
			foreach (var pe in p.PageEvents)
			{
				writePage = true;
				temp+="<b>Page Event=</b>" + pe.Name + "<BR>";
				temp+="<b>Page Text=</b>" + pe.Text + "<BR>";

				object custom = null;
				if(pe.CustomValues.ContainsKey("Custom"))
				{
					custom = pe.CustomValues["Custom"];
				}

				var obj = new MergedObj { url=p.Url, name=pe.Name, data=pe.Data, text=pe.Text, value=pe.Value, custom=pe.CustomValues};
				var json = new JavaScriptSerializer().Serialize(obj);
				temp+="<textarea id='event" + e + "' cols='160' rows='5'>" + json + "</textarea><BR>";
				temp+="<textarea id='customevt" + e + "' cols='160' rows='5'>" + custom + "</textarea><BR><BR><BR>";
				e++;
			}
			temp+="</div>";			
			if (writePage) Response.Write(temp);
		}

		Response.Write("</div>");
	}
  }
  
</script>

<head runat="server">
  <title>Mongo Page Events</title>
  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta name="CODE_LANGUAGE" content="C#" />
  <meta name="vs_defaultClientScript" content="JavaScript" />
  <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
  <link href="/default.css" rel="stylesheet" />
  <sc:VisitorIdentification runat="server" />
  <style>
  div.interaction {
    background-color: #E0FFFF;
    border-style: solid;
    border-width: 1px;	
	padding:10px 10px 10px 10px;
	margin-top: 20px;
	margin-left: 10px;
	margin-right: 10px;
  }
  
  div.page {
    background-color: #CCFFFF;
    border-style: solid;
    border-width: 1px;	
	padding:10px 10px 10px 10px;
	margin-top: 10px;
	margin-left: 10px;
	margin-right: 10px;
  }
  </style>
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
