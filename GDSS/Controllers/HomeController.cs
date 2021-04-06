using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GDSS.Controllers
{
    public class HomeController : Controller
    {
        public ModeratorController ModeratorController
        {
            get => default;
            set
            {
            }
        }

        public DiscussionController DiscussionController
        {
            get => default;
            set
            {
            }
        }

        // GET: Home
        public ActionResult LandingPage(string error = null, string message = null, string email = null)
        {
            if (Session["User"] != null)
            {
                if (Session["Role"].Equals("M"))
                    return RedirectToAction("ChooseDiscussion", "Moderator");
                else if (Session["Role"].Equals("P"))
                    return RedirectToAction("DiscussionRoom", "Discussion");
            }
            if (error != null)
                ViewBag.Error = error;
            if (message != null)
                ViewBag.Message = message;
            if (email != null)
                ViewBag.hEmail = email;
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult OnLogoClick()
        {
            return RedirectToAction("LandingPage");
        }

        public ActionResult CallVerifyEmail(string param)
        {
            TempData["reload"] = null;
            TempData["email"] = null;
            TempData["vCode"] = null;
            return RedirectToAction("VerifyEmail", "Moderator", new { param });
        }
    }
}