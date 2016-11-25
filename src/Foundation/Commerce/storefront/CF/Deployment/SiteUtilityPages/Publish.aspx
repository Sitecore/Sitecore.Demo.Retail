<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="log4net" %>
<%@ Import Namespace="Sitecore.Data.Engines" %>
<%@ Import Namespace="Sitecore.Data.Proxies" %>
<%@ Import Namespace="Sitecore.SecurityModel" %>
<%@ Import Namespace="Sitecore.Update" %>
<%@ Import Namespace="Sitecore.Update.Installer" %>
<%@ Import Namespace="Sitecore.Update.Installer.Exceptions" %>
<%@ Import Namespace="Sitecore.Data.Managers" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Publishing" %>

<%@ Language=C# %>
<HTML>
   <script runat="server" language="C#">
       public void Page_Load(object sender, EventArgs e)
       {
          Sitecore.Context.SetActiveSite("shell");
          using (new SecurityDisabler())
          {
              DateTime publishDate = DateTime.Now;
              Sitecore.Data.Database master = Sitecore.Configuration.Factory.GetDatabase("master");
              Sitecore.Data.Database web = Sitecore.Configuration.Factory.GetDatabase("web");
              var handle = PublishManager.Republish(Sitecore.Context.ContentDatabase, new Database[] { web }, LanguageManager.GetLanguages(master).ToArray(), Sitecore.Context.Language);
			  while (PublishManager.GetStatus(handle).IsDone == false)
			  {
					Thread.Sleep(5000);
			  }
          }
		  Response.Write("Publish completed: " + DateTime.Now.ToString("t") + "<br>");
       }

       protected String GetTime()
       {
           return DateTime.Now.ToString("t");
       }
   </script>
   <body>
      <form id="MyForm" runat="server">
	<div>Master DB Published.</div>
	Current server time is <% =GetTime()%>
      </form>
   </body>
</HTML>