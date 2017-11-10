using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.Security.Domains;
using Sitecore.SecurityModel.License;
using Sitecore.Shell.Framework;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.Authentication;
using System;
using System.Web;

namespace Sitecore.Support.Shell.Web
{
  /// <summary>
  /// Represents a SecurePage.
  /// </summary>
  public sealed class ShellPage
  {
    /// <summary>
    /// Determines whether this instance has license.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is logged in]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLoggedIn()
    {
      return ShellPage.IsLoggedIn(true);
    }

    /// <summary>
    /// Determines whether this instance has license.
    /// </summary>
    public static bool IsLoggedIn(bool returnAfterLogin)
    {
      HttpContext current = HttpContext.Current;
      if (!LicenseManager.HasContentManager)
      {
        string url = WebUtil.AddQueryString(Settings.NoLicenseUrl, new string[]
        {
          "license",
          "ContentManager"
        });
        if (current != null)
        {
          current.Response.Redirect(url, true);
        }
        return false;
      }
      User user = Context.User;
      if (!user.Identity.IsAuthenticated || !TicketManager.IsCurrentTicketValid() || AuthenticationManager.IsAuthenticationTicketExpired())
      {
        if (user.RuntimeSettings.IsVirtual || ShellPage.Relogin())
        {
          user = Context.User;
        }
        else
        {
          Sitecore.Shell.Framework.Security.Logout();
          ShellPage.GotoLoginPage(current, returnAfterLogin);
        }
      }
      bool flag = false;
      Domain accountDomain = Domain.GetAccountDomain(user.Name);
      if (accountDomain != null)
      {
        flag = accountDomain.IsAnonymousUser(user.Name);
      }
      if (!DomainAccessGuard.HasAccess())
      {
        if (!flag && DomainAccessGuard.IsNewUserAllowed())
        {
          DomainAccessGuard.Login(current.Session.SessionID, user.Name);
        }
        else
        {
          Sitecore.Shell.Framework.Security.Logout();
          ShellPage.GotoLoginPage(current, returnAfterLogin);
        }
      }
      if (!user.Identity.IsAuthenticated)
      {
        ShellPage.GotoLoginPage(current, returnAfterLogin);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Go to the login page.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="returnAfterLogin">if set to <c>true</c> this instance is return after login.</param>
    private static void GotoLoginPage(HttpContext httpContext, bool returnAfterLogin)
    {
      Log.Warn("Protected page accessed with no current user", typeof(ShellPage));
      UrlString urlString = new UrlString(WebUtil.GetQueryString());
      SiteContext site = Context.Site;
      if (site != null && site.LoginPage.Length > 0)
      {
        urlString.Path = site.LoginPage;
        if (returnAfterLogin)
        {
          ShellPage.SetRedirect(httpContext);
          urlString["returnUrl"] = (urlString["returnUrl"] ?? WebUtil.GetRawUrl());

          #region Added
          // Remove parameters like in the Sitecore.Pipelines.HttpRequest.ExecuteRequest.RedirectToLoginPage(String) method
          urlString.Parameters.Remove("sc_itemid");
          urlString.Parameters.Remove("user");
          urlString.Parameters.Remove("sc_site");

          #endregion
        }
        else
        {
          urlString.Parameters.Clear();
        }
        if (httpContext != null)
        {
          httpContext.Response.Redirect(urlString.ToString(), true);
          return;
        }
      }
      string str = (site != null) ? site.Name : "(unknown)";
      Error.Raise("No login page specified for current site: " + str);
    }

    /// <summary>
    /// Adds the redirect.
    /// </summary>
    /// <param name="httpContext">The context.</param>
    private static void SetRedirect(HttpContext httpContext)
    {
      Assert.ArgumentNotNull(httpContext, "httpContext");
      string text = WebUtil.GetRawUrl();
      if (text.StartsWith("/sitecore/login", StringComparison.InvariantCultureIgnoreCase))
      {
        return;
      }
      int num = text.IndexOf('?');
      if (num >= 0)
      {
        httpContext.Session["SC_LOGIN_REDIRECT"] = text;
        return;
      }
      if (text.EndsWith("/", StringComparison.InvariantCulture))
      {
        text = text.Left(text.Length - 1);
      }
      text = text.ToLowerInvariant();
      if (text == "/sitecore" || text == "/sitecore/default.aspx")
      {
        return;
      }
      if (text == "/sitecore/shell" || text == "/sitecore/shell/default.aspx")
      {
        return;
      }
      httpContext.Session["SC_LOGIN_REDIRECT"] = WebUtil.GetRawUrl();
    }

    /// <summary>
    /// Relogins the user using the specified HTTP context.
    /// </summary>
    /// <returns><c>true</c> if login attempt was successful; otherwise, <c>false</c></returns>
    private static bool Relogin()
    {
      string currentTicketId = TicketManager.GetCurrentTicketId();
      return !string.IsNullOrEmpty(currentTicketId) && TicketManager.Relogin(currentTicketId);
    }
  }
}