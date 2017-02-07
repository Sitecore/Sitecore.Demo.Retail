<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="log4net" %>
<%@ Import Namespace="Sitecore.Data.Engines" %>
<%@ Import Namespace="Sitecore.Data.Proxies" %>
<%@ Import Namespace="Sitecore.SecurityModel" %>
<%@ Import Namespace="Sitecore.Update" %>
<%@ Import Namespace="Sitecore.Update.Installer" %>
<%@ Import Namespace="Sitecore.Update.Installer.Exceptions" %>
<%@ Import Namespace="Sitecore.Data.Managers" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Search" %>
<%@ Import Namespace="Sitecore.ContentSearch" %>

<%@ Language=C# Debug="true"%>
<HTML>
   <script runat="server" language="C#">
       public void Page_Load(object sender, EventArgs e)
       {
          Sitecore.Context.SetActiveSite("shell");
          using (new SecurityDisabler())
          {
              DateTime publishDate = DateTime.Now;
			  ContentSearchManager.GetIndex("sitecore_core_index").Rebuild();
			  ContentSearchManager.GetIndex("sitecore_master_index").Rebuild();
			  ContentSearchManager.GetIndex("sitecore_web_index").Rebuild();
			  ContentSearchManager.GetIndex("commerce_products_master_index").Rebuild();
			  ContentSearchManager.GetIndex("commerce_products_web_index").Rebuild();
          }
		  Response.Write("Rebuild Index completed: " + DateTime.Now.ToString("t") + "<br>");
       }

       protected String GetTime()
       {
           return DateTime.Now.ToString("t");
       }
   </script>
   <body>
      <form id="MyForm" runat="server">
	<div>Indices rebuilt.</div>
	Current server time is <% =GetTime()%>
      </form>
   </body>
</HTML>