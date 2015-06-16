using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.Repository;
using Web.Service;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IExactClientService _exactClientService;

        public HomeController(ITokenRepository tokenRepository, IExactClientService exactClientService)
        {
            _tokenRepository = tokenRepository;
            _exactClientService = exactClientService;
        }

        public ActionResult Index()
        {
            ViewBag.DropBoxToken = Session["DropBoxAccessToken"];
            ViewBag.ExactToken = Session["ExactOnlineAccessToken"];

            return View();
        }

        // GET: /Home/ExternalLogin
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Home", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");

        }

        public ActionResult CreateDocument()
        {
            _exactClientService.initial();
            _exactClientService.CreateDocument();

            return RedirectToAction("Index");
        }

        public ActionResult DropboxSignIn()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region private

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId, string id)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId ?? id;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion

    }
}