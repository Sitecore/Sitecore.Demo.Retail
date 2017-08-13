using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Feature.Commerce.Catalog.Website.Infrastructure.Dialogs
{
    /// <summary>
    /// This dialog is overriden to cater for selecting catalog items
    /// </summary>
    public class InsertLinkForm : Sitecore.Shell.Controls.RichTextEditor.InsertLink.InsertLinkForm
    {
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");

            string url;
            string displayName;
            if (!GetSelectedUrl(out url, out displayName))
            {
                SheerResponse.Alert("Please select a valid content item or a media item.");
                return;
            }

            if (Mode == "webedit")
            {
                SheerResponse.SetDialogValue(StringUtil.EscapeJavascriptString(url));
                base.OnOK(sender, args);
            }
            else
            {
                SheerResponse.Eval("scClose(" + StringUtil.EscapeJavascriptString(url) + "," + StringUtil.EscapeJavascriptString(displayName) + ")");
            }
        }

        private bool GetSelectedUrl(out string url, out string displayName)
        {
            url = null;
            displayName = null;
            if (Tabs.Active == 0 || Tabs.Active == 2)
            {
                var selectedItem = InternalLinkTreeview.GetSelectionItem();
                if (selectedItem == null)
                {
                    return false;
                }
                displayName = selectedItem.GetUIDisplayName();
                if (selectedItem.Paths.IsMediaItem)
                {
                    url = GetMediaUrl(selectedItem);
                }
                else
                {
                    if (!IsValidContentOrProductItem(selectedItem))
                    {
                        return false;
                    }
                    var options = new LinkUrlOptions();
                    url = LinkManager.GetDynamicUrl(selectedItem, options);
                }
            }
            else
            {
                MediaItem item = MediaTreeview.GetSelectionItem();
                if (item == null)
                {
                    return false;
                }
                displayName = item.DisplayName;
                url = GetMediaUrl(item);
            }
            return true;
        }

        private static bool IsValidContentOrProductItem(Item selectedItem)
        {
            return selectedItem.Paths.IsContentItem || selectedItem.IsDerived(global::Sitecore.Foundation.Commerce.Website.Templates.Commerce.CatalogItem.Id);
        }

        private string GetMediaUrl(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return MediaManager.GetMediaUrl(item, MediaUrlOptions.GetShellOptions());
        }
    }
}