using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Web.Models;
using System.Web.Configuration;
using Owin.Security.ExactOnline;
using System.Web.Mvc;
using System.Security.Claims;
using System.Web;
using Owin.Security.Providers.Dropbox;
using System.Collections.Generic;


namespace Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/ExternalLogin")
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            RegisterAuthorizationToMiddleware(app);

          
        }

        /// <summary>
        /// Exact Online OAuth provider settings
        /// </summary>
        private static void RegisterAuthorizationToMiddleware(IAppBuilder app)
        {
            var exactOnlineEndpoint = WebConfigurationManager.AppSettings["ExactOnlineEndpoint"];

            app.UseExactOnlineAuthentication(
                new ExactOnlineAuthenticationOptions("ExactOnlineNL")
                {
                    TokenEndpoint = string.Format("{0}/api/oauth2/token", exactOnlineEndpoint),
                    AuthEndpoint = string.Format("{0}/api/oauth2/auth", exactOnlineEndpoint),
                    UserInfoEndpoint = string.Format("{0}/api/system/System.svc/Me", exactOnlineEndpoint),
                    Caption = "Get ExactOnline Token",
                    ClientId = WebConfigurationManager.AppSettings["ExactOnlineClientId"],
                    ClientSecret = WebConfigurationManager.AppSettings["ExactOnlineClientSecret"],
                    Provider = new ExactOnlineAuthenticationProvider
                    {
                        OnAuthenticated = async context =>
                        {
                            //context.Identity.AddClaim(new Claim("ExactOnline:accesstoken", context.AccessToken));
                            var userId = Guid.Parse(context.UserID);
                            var credential = new Credential
                            {
                                ProviderUserKey = userId,
                                Provider = "ExactOnline",
                                AccessToken = context.AccessToken,
                                RefreshToken = context.RefreshToken,
                                ExpiresAt = CalculateExpireDate(context.ExpiresIn)
                            };

                            HttpContext.Current.Session["Credential"] = credential;
                            HttpContext.Current.Session["ExactOnlineAccessToken"] = context.AccessToken;
                        }
                    }
                });

            var options = new DropboxAuthenticationOptions
            {
                AppKey = WebConfigurationManager.AppSettings["DropBoxAppKey"],
                AppSecret = WebConfigurationManager.AppSettings["DropBoxAppSecret"],
                CallbackPath = new PathString("/Home/DropboxSignIn"),
                Provider = new DropboxAuthenticationProvider
                {
                    OnAuthenticated = async context =>
                    {
                        var credential = new Credential
                        {
                            Provider = "DropBox",
                            AccessToken = context.AccessToken
                        };

                        HttpContext.Current.Session["DropBoxAccessToken"] = context.AccessToken;
                    }
                }
            };

            app.UseDropboxAuthentication(options);
        }

        private static DateTime CalculateExpireDate(TimeSpan? expireValue)
        {
            return !expireValue.HasValue ? DateTime.Now : DateTime.Now.AddMinutes(expireValue.Value.Minutes);
        }

    }
}