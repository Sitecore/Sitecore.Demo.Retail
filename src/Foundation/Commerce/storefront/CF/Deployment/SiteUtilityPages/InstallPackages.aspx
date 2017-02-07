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
<%@ Import Namespace="Sitecore.Update.Installer.Installer.Utils" %>
<%@ Import Namespace="Sitecore.Update.Installer.Utils" %>
<%@ Import Namespace="Sitecore.Update.Metadata" %>
<%@ Import Namespace="Sitecore.Update.Utils" %>
<%@ Import Namespace="Sitecore.Web" %>

<%@ Language=C# Debug="true" %>
<HTML>
   <script runat="server" language="C#">
    public void Page_Load(object sender, EventArgs e)
    {
	  var file = Server.MapPath("/sitecore/admin/Packages") + "/" + WebUtil.GetQueryString("package");
      Sitecore.Context.SetActiveSite("shell");
      using (new SecurityDisabler())
      {
        using (new ProxyDisabler())
        {
          using (new SyncOperationContext())
          {
              var result = Install(file);
              Response.Write("Installed Package: " + file + "<br>");
			  Response.Write("Result: " + result + "<br>");
          }
        }
      }
    }

    protected static string Install(string package)
    {
      var log = LogManager.GetLogger("LogFileAppender");
      string result;
      using (new ShutdownGuard())
      {
		var installationInfo = new PackageInstallationInfo();
		installationInfo.Action = UpgradeAction.Upgrade;
		installationInfo.Mode = InstallMode.Install;
		installationInfo.Path = package;
	
		var res = installationInfo.GetType().GetProperty("ProcessingMode");
		if (res != null) {
			var e = installationInfo.GetType().GetProperty("ProcessingMode");
			e.SetValue(installationInfo, 0x7fffffff);	
		} 

        string text = null;
        List<ContingencyEntry> entries = null;
        try
        {
          entries = UpdateHelper.Install(installationInfo, log, out text);
		  MetadataView view = UpdateHelper.LoadMetadata(installationInfo.Path);
		  var installer = new DiffInstaller(installationInfo.Action);
		  installer.ExecutePostInstallationInstructions(installationInfo.Path, text, installationInfo.Mode, view, log, ref entries);
        }
        catch (PostStepInstallerException ex)
        {
          entries = ex.Entries;
          text = ex.HistoryPath;
          throw;
        }
        finally
        {
          //UpdateHelper.SaveInstallationMessages(entries, text);
        }

        result = text;
      }

      return result;
    }
    
    protected String GetTime()
    {
        return DateTime.Now.ToString("t");
    }
   </script>
   <body>
      <form id="MyForm" runat="server">
	<div>This page installs packages from \sitecore\admin\Packages folder.</div>
	Current server time is <% =GetTime()%>
      </form>
   </body>
</HTML>