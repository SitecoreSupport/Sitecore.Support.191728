using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.SecurityModel;
using Sitecore.Shell.Web;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.Bundling;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using System;
using System.Collections.Generic;
using System.Web.Helpers;
using System.Web.UI;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.PageExtender
{
  public class RibbonWebControl : Sitecore.ExperienceEditor.Speak.Ribbon.PageExtender.RibbonWebControl
  {
    private static readonly string RibbonItemPath = "/sitecore/client/Applications/ExperienceEditor/Ribbon";

    private string state;

    protected override void DoRender(HtmlTextWriter output)
    {
      SiteContext site = Factory.GetSite("shell");
      using (new SiteContextSwitcher(site))
      {
        // Uses patched ShellPage class instead of original to fix the GotoLoginPage method
        Sitecore.Support.Shell.Web.ShellPage.IsLoggedIn();
      }
      string mode = this.Mode;
      bool webEdit = mode == "edit";
      bool debug = mode == "debug";
      List<string> list = ScriptResources.GenerateBaseContentEditingScriptsList(webEdit, debug);
      foreach (string current in list)
      {
        output.Write(HtmlUtil.GetClientScriptIncludeHtml(current));
      }
      if (WebUtility.IsSublayoutInsertingMode)
      {
        return;
      }
      this.RenderResources(output);
      output.Write("<link href=\"{0}\" rel=\"stylesheet\" />", Sitecore.Configuration.Settings.WebEdit.ContentEditorStylesheet);
      WebUtility.RenderLoadingIndicator(output);
      output.Write("<link href=\"{0}\" rel=\"stylesheet\" />", Sitecore.ExperienceEditor.Settings.WebEdit.ExperienceEditorStylesheet);
      string value = string.Format("<iframe id=\"scWebEditRibbon\" src=\"{0}\" class=\"scSpeakWebEditRibbon scWebEditRibbon scFixedRibbon\" frameborder=\"0\" marginwidth=\"0\" marginheight=\"0\" height=\"50px\" width=\"100%\"></iframe>", this.Url);
      output.Write(value);
      string device = WebUtility.GetDevice(new UrlString(this.Url));
      WebUtility.RenderLayout(Sitecore.Context.Item, output, Sitecore.Context.Site.Name, device);
      if (Sitecore.Context.Site.DisplayMode == DisplayMode.Edit)
      {
        List<string> capabilities = RibbonWebControl.GetCapabilities();
        output.Write("<input type=\"hidden\" id=\"scCapabilities\" value=\"" + string.Join("|", capabilities.ToArray()) + "\" />");
      }
      output.Write("<input type=\"hidden\" id=\"scLayoutDefinition\" name=\"scLayoutDefinition\" />");
      output.Write(AntiForgery.GetHtml());
    }
    
    private static List<string> GetCapabilities()
    {
      List<string> list = new List<string>();
      if (Policy.IsAllowed("Page Editor/Can Design") && Registry.GetString("/Current_User/Page Editor/Capability/design") != Sitecore.ExperienceEditor.Constants.Registry.CheckboxUnTickedRegistryValue)
      {
        list.Add("design");
      }
      if (Policy.IsAllowed("Page Editor/Can Edit") && Registry.GetString("/Current_User/Page Editor/Capability/edit") != Sitecore.ExperienceEditor.Constants.Registry.CheckboxUnTickedRegistryValue)
      {
        list.Add("edit");
      }
      if (Policy.IsAllowed("Page Editor/Extended features/Personalization"))
      {
        list.Add("personalization");
      }
      if (Policy.IsAllowed("Page Editor/Extended features/Testing"))
      {
        list.Add("testing");
      }
      return list;
    }
  }
}