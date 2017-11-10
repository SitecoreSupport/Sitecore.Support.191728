using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.Shell.Web;
using Sitecore.Sites;
using System;

namespace Sitecore.Support.Mvc.Pipelines.Request.RequestBegin
{
  public class EnsureLoggedInForPreview
  {
    public void Process(PipelineArgs args)
    {
      if (!Context.PageMode.IsNormal && !Context.IsLoggedIn)
      {
        using (new SiteContextSwitcher(Factory.GetSite("shell")))
        {
          ShellPage.IsLoggedIn();
        }
      }
    }
  }
}