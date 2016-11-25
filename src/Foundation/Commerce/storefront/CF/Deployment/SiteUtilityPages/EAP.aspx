<%@ Import Namespace="Sitecore.Analytics"%>
<%@ Import Namespace="Sitecore.Analytics.Automation"%>
<%@ Import Namespace="Sitecore.Analytics.Automation.MarketingAutomation"%>
<%@ Import Namespace="Sitecore.Web" %>
<%@ Page Language="c#" Inherits="System.Web.UI.Page" CodePage="65001" Debug="true"%>
<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.WebControls" Assembly="Sitecore.Kernel" %>
<%@ OutputCache Location="None" VaryByParam="none" %>
<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">

<script runat="server">
  public void WriteTestResults()
  {
	  var timeoutPageEventId = new Sitecore.Data.ID("F8934EDC-A01D-4554-9CCA-D33190441CDE");
	  var contactId = WebUtil.GetQueryString("contact");
	  var trigger = WebUtil.GetQueryString("trigger");
	  var stateId = WebUtil.GetQueryString("state");
		  
	  Sitecore.Analytics.Automation.Data.AutomationStateManager am;
	  Sitecore.Analytics.Tracking.Contact myContact;
	  
	  if (String.IsNullOrEmpty(contactId.ToString()))
	  {
			am = Sitecore.Analytics.Tracker.Current.Session.CreateAutomationStateManager();
			myContact = am.Contact;
	  } 
	  else
          {
			myContact = Sitecore.Commerce.Automation.MarketingAutomation.CommerceAutomationHelper.GetContact(contactId);
			am = Sitecore.Analytics.Automation.Data.AutomationStateManager.Create(myContact);	  
	  }

	  var states = am.GetAutomationStates();

	  Response.Write("Contact id: ");
	  Response.Write("<input type=text name='Contactid' id='contactid' value='" + myContact.ContactId + "'>");
	  Response.Write("<br>");
	  Response.Write("Identifier: ");
	  Response.Write("<input type=text name='Identifier' id='identifier' value = '" + myContact.Identifiers.Identifier + "'>");
	  Response.Write("<br>"); 

	  foreach (var s in states)
	  {
		Response.Write("================================<BR>"); 
		Response.Write("PlanId= " + s.PlanId + "<BR>"); 
		if (s.PlanItem != null)
			Response.Write("Plan Name= " + s.PlanItem.Name + "<BR>"); 
		if (s.StateItem != null)
		{
			Response.Write("CurrentStateId= " + s.StateItem.ID + "<BR>");
			Response.Write("CurrentStateName= " + s.StateItem.Name + "<BR>");
			Response.Write("WakeUpDateTime= " + s.WakeUpDateTime.ToString() + "<BR>");
			
			// Force Trigger
			bool forceTrigger = false;
			if (!String.IsNullOrEmpty(trigger.ToString()))
			{
				if (String.IsNullOrEmpty(stateId)) 
					forceTrigger = true;
				else if (s.StateItem.ID.ToString() == stateId)
					forceTrigger = true;
			}
			
			if (forceTrigger)
			{
				s.LastAccessedDateTime = DateTime.Now;
				s.WakeUpDateTime = DateTime.Now;
				Response.Write("Force Triggering...<BR>");
				AutomationContactManager.ForceTrigger(myContact.ContactId, timeoutPageEventId, s.StateItem.ID);
			}
		} 
	  }
  }
</script>

<head runat="server">
  <title>EAPs</title>
  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta name="CODE_LANGUAGE" content="C#" />
  <meta name="vs_defaultClientScript" content="JavaScript" />
  <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
  <link href="/default.css" rel="stylesheet" />
  <sc:VisitorIdentification runat="server" />
  <style>
  input[type="text"] {
    width: 200px;
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
