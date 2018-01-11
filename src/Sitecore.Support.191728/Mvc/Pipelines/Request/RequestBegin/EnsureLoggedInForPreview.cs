using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.Security.Authentication;
using Sitecore.Shell.Web;
using Sitecore.Sites;
using Sitecore.Web.Authentication;
using System;

namespace Sitecore.Support.Mvc.Pipelines.Request.RequestBegin
{
  public class EnsureLoggedInForPreview
  {
    public void Process(PipelineArgs args)
    {
      if ((!Context.PageMode.IsNormal && !Context.IsLoggedIn)
          || !TicketManager.IsCurrentTicketValid()
          || AuthenticationManager.IsAuthenticationTicketExpired())
            {
        using (new SiteContextSwitcher(Factory.GetSite("shell")))
        {
          // Use patched ShellPage class instead of original
          Sitecore.Support.Shell.Web.ShellPage.IsLoggedIn();
        }
      }
    }
  }
}