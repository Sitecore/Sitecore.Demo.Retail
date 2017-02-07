<%@ Import Namespace="System" %>
<%@ Import Namespace="Sitecore.Configuration" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Data.Fields" %>
<%@ Import Namespace="Sitecore.Data.Items" %>
<%@ Import Namespace="Sitecore.Layouts" %>
<%@ Import Namespace="Sitecore.SecurityModel" %>
<%@ Import Namespace="Sitecore.Web" %>

<%@ Language=C# %>
<HTML>
   <script runat="server" language="C#">
        public const string DefaultDeviceId = "{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}";
        public const string Database = "master";
        public const string StorefrontPath = "/sitecore/content/Storefront";
        public const string StorefrontName = "Storefront";
        public const string EngagementPlanPath = "/sitecore/system/Marketing Control Panel/Engagement Plans/CommerceConnect";

        public string StorefrontNewName;
        public string StorefrontNewPath;
        public string NewCatalogName;
        public string NewCatalogPath;
        public string NewCatalogDatasourcePath;
        

		public void Page_Load(object sender, EventArgs e)
		{
            StorefrontNewName = WebUtil.GetQueryString("name");
            StorefrontNewPath = "/sitecore/content/" + StorefrontNewName;
            NewCatalogName = WebUtil.GetQueryString("catalogname");
            NewCatalogPath = "/sitecore/Commerce/Catalog Management/Catalogs/" + NewCatalogName;
            NewCatalogDatasourcePath = WebUtil.GetQueryString("catalogdatasourcepath").Trim('"');
            
			Sitecore.Context.SetActiveSite("shell");
			using (new SecurityDisabler())
			{
				this.CreateStorefront();
			}
		}
		
        public void CreateStorefront()
        {
            Database db = Factory.GetDatabase(Database);

            // === Step 1: Duplicate
			Response.Write("Duplicating storefront <br>");
            if (db.GetItem(StorefrontNewPath) == null)
            {
                this.DuplicateItem(StorefrontPath, StorefrontNewName);
            }

            // === Step 2: Update datasource on home node and children
			Response.Write("Updating new storefront datasource properties in layout <br>");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home");
            this.UpdateDatasourcePropertyOnItemAndChildren(StorefrontNewPath + "/Home/AccountManagement");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Category/*");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Error");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/ForgotPassword");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/ForgotPasswordConfirmation");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Login");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Register");
            this.UpdateDatasourcePropertyOnItemAndChildren(StorefrontNewPath + "/Home/Shop/*");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/ShoppingCart");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Store catalogs");
            this.UpdateDatasourcePropertyOnItemAndChildren(StorefrontNewPath + "/Home/Company");
            this.UpdateDatasourcePropertyOnItemAndChildren(StorefrontNewPath + "/Home/Checkout");
            this.UpdateDatasourcePropertyOnItem(StorefrontNewPath + "/Home/Search");

            // === Step 3: Update Global Items
            // Update all of the lists to the right items
			Response.Write("Updating new storefront global items <br>");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/Carousels/Carousel Frontpage", "Slides");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/QuickLinks/TopBarLinks", "QuickLinks");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/QuickLinks/Account information", "QuickLinks");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/QuickLinks/Company information", "QuickLinks");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/QuickLinks/Site options", "QuickLinks");
            this.UpdateItemLinksListForField(StorefrontNewPath + "/Global/QuickLinks/Follow us", "QuickLinks");

            // Update all of the links
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/About Storefront", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Account dashboard", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Corporate sales", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Frequently asked questions", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Jobs at Storefront", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Order status", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/Store catalogs", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/Footer/View mobile site", "Link");
            this.UpdateItemLinkField(StorefrontNewPath + "/Global/QuickLinks/Links/TopBar/Store catalogs", "Link");

            // Remove all named searches
            this.EditItemField(StorefrontNewPath + "/Global/Product Search Lists/Recommended Products", "Named Searches", "");
            this.DeleteItem(StorefrontNewPath + "/Global/Named Searches/On Sale");
            this.DeleteItem(StorefrontNewPath + "/Global/Named Searches/Shop by brand");
            
            // Fix footer navigation
            this.UpdateItemFieldId(StorefrontNewPath + "/Global/Site Spots/Footer Navigation", "QuickLinks Left");
            this.UpdateItemFieldId(StorefrontNewPath + "/Global/Site Spots/Footer Navigation", "QuickLinks Middle Left");
            this.UpdateItemFieldId(StorefrontNewPath + "/Global/Site Spots/Footer Navigation", "QuickLinks Middle Right");
            this.UpdateItemFieldId(StorefrontNewPath + "/Global/Site Spots/Footer Navigation", "QuickLinks Right");

            // === Step 4: Update Misc Items
			Response.Write("Updating new storefront properties <br>");
            // Enabled Catalogs
            string selectedCatalogs = this.GetItemFieldValue("/sitecore/Commerce/Catalog Management/Catalogs", "Selected Catalogs");

            if (!selectedCatalogs.Contains(NewCatalogName))
            {
                selectedCatalogs = selectedCatalogs + "|" + NewCatalogName;
                this.EditItemField("/sitecore/Commerce/Catalog Management/Catalogs", "Selected Catalogs", selectedCatalogs);
            }

            // Catalog Datasource
            Item catalog = db.GetItem(NewCatalogPath);
            this.EditItemField(StorefrontNewPath + "/Home", "Catalogs", catalog.ID.Guid.ToString("B")); // A Guid
            this.EditItemField(StorefrontNewPath + "/Home/Product catalog", "CategoryDatasource", NewCatalogDatasourcePath);

            // === Step 5: Copy engagement plans
            if (db.GetItem(EngagementPlanPath + "/" + StorefrontNewName + " Abandoned Carts") == null)
            {
                this.DuplicateItem(EngagementPlanPath + "/" + StorefrontName + " Abandoned Carts", StorefrontNewName + " Abandoned Carts");
            }

            if (db.GetItem(EngagementPlanPath + "/" + StorefrontNewName + " New Order Placed") == null)
            {
                this.DuplicateItem(EngagementPlanPath + "/" + StorefrontName + " New Order Placed", StorefrontNewName + " New Order Placed");
            }

            if (db.GetItem(EngagementPlanPath + "/" + StorefrontNewName + " Products Back In Stock") == null)
            {
                this.DuplicateItem(EngagementPlanPath + "/" + StorefrontName + " Products Back In Stock", StorefrontNewName + " Products Back In Stock");
            }
			
			Response.Write("Done! <br>");
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="itemIdOrPath">The item id or path</param>
        private void DeleteItem(string itemIdOrPath)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                if (item != null)
                {
                    item.Delete();
                }
            }
        }

        /// <summary>
        /// Update an ID for an item field to point to the new storefront
        /// </summary>
        /// <param name="itemIdOrPath">The item id or path</param>
        /// <param name="fieldName">The field where the id is stored</param>
        private void UpdateItemFieldId(string itemIdOrPath, string fieldName)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                string id = item[fieldName];
                string path = db.GetItem(id).Paths.FullPath;

                if (path.Contains("/" + StorefrontName + "/"))
                {
                    string newId = db.GetItem(path.Replace("/" + StorefrontName + "/", "/" + StorefrontNewName + "/")).ID.Guid.ToString("B");

                    item.Editing.BeginEdit();

                    item[fieldName] = newId;

                    item.Editing.EndEdit();
                }
            }
        }

        /// <summary>
        /// Update the item link field to point to the new storefront
        /// Link contains raw HTML with an ID attribute
        /// </summary>
        /// <param name="itemIdOrPath">The item id or path</param>
        /// <param name="fieldName">The name of the link field</param>
        private void UpdateItemLinkField(string itemIdOrPath, string fieldName)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                string link = item[fieldName];
                string id = link.Substring(link.IndexOf("id=") + 4, 38);
                string path = db.GetItem(id).Paths.FullPath;

                if (path.Contains("/" + StorefrontName + "/"))
                {
                    string newId = db.GetItem(path.Replace("/" + StorefrontName + "/", "/" + StorefrontNewName + "/")).ID.Guid.ToString("B");

                    string newLink = link.Replace(id, newId);

                    item.Editing.BeginEdit();

                    item[fieldName] = newLink;

                    item.Editing.EndEdit();
                }
            }
        }

        /// <summary>
        /// Update a list of links to sitecore items for a field to use the new storefront path
        /// </summary>
        /// <param name="itemIdOrPath">The id or path of the item</param>
        /// <param name="fieldName">The name of hte fields where the links are stored</param>
        private void UpdateItemLinksListForField(string itemIdOrPath, string fieldName)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                string[] links = item[fieldName].Split('|');
                string newLinks = string.Empty;

                foreach (string link in links)
                {
                    string linkPath = db.GetItem(link).Paths.FullPath;

                    if (linkPath.Contains("/" + StorefrontName + "/"))
                    {
                        linkPath = linkPath.Replace("/" + StorefrontName + "/", "/" + StorefrontNewName + "/");

                        if (!string.IsNullOrEmpty(newLinks))
                        {
                            newLinks += "|";
                        }

                        newLinks += db.GetItem(linkPath).ID.Guid.ToString("B");
                    }
                }

                if (!string.IsNullOrEmpty(newLinks))
                {
                    item.Editing.BeginEdit();

                    item[fieldName] = newLinks;

                    item.Editing.EndEdit();
                }
            }
        }

        /// <summary>
        /// Create a copy of an item and all sub-items
        /// </summary>
        /// <param name="path">The path of the item to copy</param>
        /// <param name="newName">The name of the item (will be placed in same path)</param>
        private void DuplicateItem(string path, string newName)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);
                Item item = db.GetItem(path);

                item.Duplicate(newName);
            }
        }

        /// <summary>
        /// Gets the item field value
        /// </summary>
        /// <param name="itemIdOrPath">The id or path of the item</param>
        /// <param name="fieldName">The field name</param>
        /// <returns>The field value</returns>
        private string GetItemFieldValue(string itemIdOrPath, string fieldName)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                return item[fieldName];
            }
        }

        /// <summary>
        /// Edit an item field
        /// </summary>
        /// <param name="itemIdOrPath">The id or path of the item</param>
        /// <param name="fieldName">The field name</param>
        /// <param name="fieldValue">The field value</param>
        private void EditItemField(string itemIdOrPath, string fieldName, string fieldValue)
        {
            using (new SecurityDisabler())
            {
                Database db = Factory.GetDatabase(Database);

                Item item = db.GetItem(itemIdOrPath);

                item.Editing.BeginEdit();

                item[fieldName] = fieldValue;

                item.Editing.EndEdit();
            }
        }

        /// <summary>
        /// Update the datasource property for an item and it's children
        /// </summary>
        /// <param name="path">The item path</param>
        private void UpdateDatasourcePropertyOnItemAndChildren(string path)
        {
            using (new SecurityDisabler())
            {
                // Get the database
                Database db = Factory.GetDatabase(Database);

                // Get the item
                Item item = db.GetItem(path);

                this.UpdateDatasourcePropertyOnItem(item.Paths.FullPath);

                if (item.HasChildren)
                {
                    foreach (Item child in item.Children)
                    {
                        this.UpdateDatasourcePropertyOnItem(child.Paths.FullPath);
                    }
                }
            }
        }

        /// <summary>
        /// Update the datasource property for a specific item
        /// </summary>
        /// <param name="path">The path to the item</param>
        private void UpdateDatasourcePropertyOnItem(string path)
        {
            using (new SecurityDisabler())
            {
                // Get the database
                Database db = Factory.GetDatabase(Database);

                // Get the item
                Item item = db.GetItem(path);

                // Get the layout field
                var field = item.Fields[Sitecore.FieldIDs.LayoutField];

                string xml = LayoutField.GetFieldValue(field);
                var details = Sitecore.Layouts.LayoutDefinition.Parse(xml);

                // Get the device
                DeviceDefinition myDevice = details.GetDevice(DefaultDeviceId);

                if (myDevice.Renderings == null)
                {
                    return;
                }

                bool changesNeedSaving = false;

                // Cycle through all renderings to set the datasource
                foreach (RenderingDefinition rendering in myDevice.Renderings)
                {
                    if (!string.IsNullOrEmpty(rendering.Datasource))
                    {
                        string datasourcePath = db.GetItem(rendering.Datasource).Paths.FullPath;

                        if (datasourcePath.Contains("/" + StorefrontName + "/"))
                        {
                            datasourcePath = datasourcePath.Replace("/" + StorefrontName + "/", "/" + StorefrontNewName + "/");
                            datasourcePath = db.GetItem(datasourcePath).ID.Guid.ToString("B");

                            rendering.Datasource = datasourcePath;
                            changesNeedSaving = true;
                        }
                    }
                }

                if (changesNeedSaving)
                {
                    // Save the changes
                    string newXml = details.ToXml();

                    using (new Sitecore.Data.Items.EditContext(item))
                    {
                        LayoutField.SetFieldValue(field, newXml);
                    }
                }
            }
        }

		protected String GetTime()
		{
			return DateTime.Now.ToString("t");
		}
   </script>
   <body>
      <form id="MyForm" runat="server">
	<div>Storefront successfully created</div>
	Current server time is <% =GetTime()%>
      </form>
   </body>
</HTML>