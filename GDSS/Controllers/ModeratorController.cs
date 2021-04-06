using GDSS.Models;
using System;
using System.Web.Mvc;
using DocumentFormat.OpenXml.InkML;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace GDSS.Controllers
{
    public class ModeratorController : Controller
    {
        GDSS_DBEntities dBEntities = new GDSS_DBEntities();

        public Discussion Discussion
        {
            get => default;
            set
            {
            }
        }

        public Moderator Moderator
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

        public Forum Forum
        {
            get => default;
            set
            {
            }
        }

        public Participant Participant
        {
            get => default;
            set
            {
            }
        }

        public Poll Poll
        {
            get => default;
            set
            {
            }
        }

        public ActionResult ModeratorSignUp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public ActionResult PerformSignUp([Bind(Include = "firstname,  lastname,  email,  password")] Moderator moderator,
            [Bind(Include = "Topic,  Description,  ClientName,  ClientEmail, StartDateTime,  DeletionDateTime")] Discussion discussion)
        {

            dBEntities.Moderators.Add(moderator);
            discussion.Moderator = moderator;
            discussion.Status = "Upcoming";
            dBEntities.Discussions.Add(discussion);
            dBEntities.SaveChanges();
            Session["User"] = moderator.email;
            Session["Role"] = "M";
            Session["Discussion"] = discussion.Id;
            return RedirectToAction("ModeratorRoom");
        }

        public ActionResult CreateDiscussion()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PerformCreateDiscussion([Bind(Include = "Topic,  Description,  ClientName,  ClientEmail, StartDateTime,  DeletionDateTime")] Discussion discussion)
        {
            @Moderator moderator = dBEntities.Moderators.Find(Session["User"]);
            discussion.Moderator = moderator;
            discussion.Status = "Upcoming";
            dBEntities.Discussions.Add(discussion);
            dBEntities.SaveChanges();
            Session["Discussion"] = discussion.Id;
            return RedirectToAction("ModeratorRoom");
        }

        public ActionResult CallModeratorRoom(int discussionID)
        {
            Session["Discussion"] = discussionID;
            return RedirectToAction("ModeratorRoom");
        }

       
        public ActionResult VerifyEmail(string param, bool resend = false)
        {
            Uri referrer = Request.UrlReferrer;

            if (referrer.AbsolutePath.Contains("LandingPage") || (bool?)TempData["reload"] == null || resend)
            {
                try
                {
                    string vEmail = param;
                    if (!resend)
                    {
                        if (vEmail.Trim().Equals(""))
                            return RedirectToAction("LandingPage", "Home", new { error = "Please enter a valid email address.", email = vEmail });
                        @Moderator m = dBEntities.Moderators.Find(vEmail);
                        if (m != null)
                        {
                            return RedirectToAction("LandingPage", "Home", new { error = "User already registered. Please log in.", email = vEmail });
                        }
                    }
                    TempData["reload"] = true;
                    TempData["email"] = param;
                    Random random = new Random();
                    long vCode = random.Next(101213, 986790);
                    TempData["vCode"] = vCode;
                    MailAddress senderAddress = new MailAddress("teambigmacs5@gmail.com", "Team Cloak");
                    MailAddress receiverAddress = new MailAddress(vEmail);
                    string senderPassword = "abeykurian";
                    string subject = "Verify your email";
                    string body = "Thank you for choosing Cloak! " +
                        "To verify your email, please enter the following code in your signup page:<br/><br/><b style='font-size:1.5em'>"
                        + vCode + "</b><br/><br/>Get discussing!";
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderAddress.Address, senderPassword)
                    };

                    using (
                        MailMessage message = new MailMessage(senderAddress, receiverAddress)
                        {
                            Subject = subject,
                            Body = body
                        }
                            )
                    {
                        message.IsBodyHtml = true;
                        client.Send(message);
                    }
                    if (resend)
                    {
                        resend = false;
                        return Content("A new verification code has been sent to your email address. The previous code has expired.", "text/plain");
                    }
                    else
                    {
                        ViewBag.Email = vEmail;
                        return View();
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("LandingPage", "Home", new { error = "An error occured. Please try again." });
                }
            }
            else if (referrer.AbsolutePath.Contains("LandingPage") && ((bool?)TempData["reload"] != null && (bool?)TempData["reload"] == true))
            {
                TempData.Keep("reload");
                ViewBag.Email = TempData["email"];
                TempData.Keep("email");
                return View();
            }
            else
            {
                long uCode;
                try
                {
                    uCode = Int64.Parse(param); ;
                }
                catch (Exception)
                {
                    TempData.Keep("vCode");
                    ViewBag.Email = TempData["email"];
                    TempData.Keep("email");
                    TempData["reload"] = true;
                    ViewBag.Error = "Please enter a six-digit numerical code.";
                    return View();
                }
                long vCode = (long)TempData["vCode"];
                if (uCode == vCode)
                    return RedirectToAction("ModeratorSignUp", new { email = TempData["email"] });
                else
                {
                    TempData.Keep("vCode");
                    ViewBag.Email = TempData["email"];
                    TempData.Keep("email");
                    TempData["reload"] = true;
                    ViewBag.Error = "Incorrect code. Please try again.";
                    return View();
                }
            }

        }


        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            string message;
            if (email.Trim().Equals(""))
            {
                if (password.Trim().Equals(""))
                    message = "Email address and password required";
                else message = "Email address required";
            }
            else if (password.Trim().Equals(""))
            {
                message = "Password required";
            }

            else if (ModelState.IsValid)
            {
                @Moderator m = dBEntities.Moderators.Find(email);
                if (m != null)
                {
                    if (m.password == password)
                    {
                        Session["User"] = email;
                        Session["Role"] = "M";
                        return RedirectToAction("ChooseDiscussion");
                    }
                    else message = "Wrong password";
                }
                else message = "User does not exist";
            }
            else message = "An error occured. Please try again.";
            return RedirectToAction("LandingPage", "Home", new { message, email });
        }

        public ActionResult ModeratorRoom()
        {
            @Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            if (d.Status.Equals("Upcoming"))
            {
                DateTime currentDateTime = DateTime.Now;
                if (d.StartDateTime <= currentDateTime)
                {
                    d.Status = "Active";
                    dBEntities.SaveChanges();
                }
            }
            ViewBag.Discussion = d;
            return View();
        }


        public ActionResult ResetPassword(string param, string password = null, bool resend = false)
        {
            Uri referrer = Request.UrlReferrer;

            if (referrer.AbsolutePath.Contains("EnterEmail") || (bool?)TempData["reload"] == null || resend)
            {
                if (!resend)
                {
                    if (param.Trim().Equals(""))
                        return RedirectToAction("EnterEmail", new { error = "Please enter a valid email address." });
                    @Moderator m = dBEntities.Moderators.Find(param);
                    if (m == null)
                    {
                        return RedirectToAction("EnterEmail", new { error = "User does not exist." });
                    }
                }
                try
                {
                    TempData["reload"] = true;
                    string vEmail = param;
                    TempData["email"] = param;
                    Random random = new Random();
                    long vCode = random.Next(101213, 986790);
                    TempData["vCode"] = vCode;
                    MailAddress senderAddress = new MailAddress("teambigmacs5@gmail.com", "Team Cloak");
                    MailAddress receiverAddress = new MailAddress(vEmail);
                    string senderPassword = "abeykurian";
                    string subject = "Reset your password";
                    string body = "To reset your password, please enter the following code in your password reset page:<br/><br/><b style='font-size:1.5em'>"
                        + vCode + "</b><br/><br/>Get discussing!";
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderAddress.Address, senderPassword)
                    };

                    using (
                        MailMessage message = new MailMessage(senderAddress, receiverAddress)
                        {
                            Subject = subject,
                            Body = body
                        }
                            )
                    {
                        message.IsBodyHtml = true;
                        client.Send(message);
                    }
                    if (resend)
                    {
                        resend = false;
                        return Content("A new verification code has been sent to your email address. The previous code has expired.", "text/plain");
                    }
                    else
                    {
                        ViewBag.Email = vEmail;
                        return View();
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("EnterEmail", new { error = "An error occured. Please try again." });
                }
            }
            else if (referrer.AbsolutePath.Contains("EnterEmail") && ((bool?)TempData["reload"] != null && (bool?)TempData["reload"] == true))
            {
                TempData.Keep("reload");
                ViewBag.Email = TempData["email"];
                TempData.Keep("email");
                return View();
            }
            else
            {
                long uCode;
                try
                {
                    uCode = Int64.Parse(param); ;
                }
                catch (Exception)
                {
                    TempData.Keep("vCode");
                    ViewBag.Email = TempData["email"];
                    TempData.Keep("email");
                    ViewBag.Error = "Please enter a six-digit numerical code.";
                    return View();
                }
                long vCode = (long)TempData["vCode"];
                if (uCode == vCode)
                {
                    @Moderator m = dBEntities.Moderators.Find(TempData["email"]);
                    m.password = password;
                    dBEntities.SaveChanges();
                    Session["User"] = TempData["email"];
                    Session["Role"] = "M";
                    return RedirectToAction("ChooseDiscussion");
                }
                else
                {
                    TempData.Keep("vCode");
                    ViewBag.Email = TempData["email"];
                    TempData.Keep("email");
                    ViewBag.Error = "Incorrect code. Please try again.";
                    return View();
                }
            }
        }


        public ActionResult EnterEmail(string error = null)
        {
            if (error != null)
                ViewBag.Error = error;
            return View();
        }

        public ActionResult Tools()
        {
            return View();
        }

        public ActionResult ChooseDiscussion()
        {
            string user = (string)Session["User"];
            List<Discussion> d = new List<Discussion>(dBEntities.Discussions.Where(x => x.ModeratorEmail == user));
            ViewBag.Discussions = d.ToArray();
            return View();
        }

        public ActionResult Logout()
        {
            Session["User"] = null;
            Session["Discussion"] = null;
            Session["Role"] = null;
            return RedirectToAction("LandingPage", "Home");
        }

        public ActionResult InviteParticipants()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PerformInvite(string[] name, string[] email)
        {
            int discussionID = (int)Session["Discussion"];
            string discussionTopic = dBEntities.Discussions.Find(discussionID).Topic;
            string discussionDescription = dBEntities.Discussions.Find(discussionID).Description;
            List<string> tempEmails = new List<string>();

            MailAddress senderAddress = new MailAddress("teambigmacs5@gmail.com", "Team Cloak");
            string senderPassword = "abeykurian";
            string subject = "Discussion Invitation";

            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[32];



            SmtpClient client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderAddress.Address, senderPassword)
            };



            for (int i = 0; i < email.Length; i++)
            {
                string thisEmail = email[i];
                if (!tempEmails.Contains(thisEmail))
                {
                    @Participant p = dBEntities.Participants.FirstOrDefault(x => x.DiscussionID == discussionID && x.Email == thisEmail);
                    if (p == null)
                    {
                        rngCryptoServiceProvider.GetBytes(randomBytes);
                        string token = Base64UrlEncoder.Encode(Convert.ToBase64String(randomBytes));
                        p = new Participant()
                        {
                            Name = name[i],
                            Email = email[i],
                            Discussion = dBEntities.Discussions.Find(discussionID),
                            Status = "Invited",
                            Token = token
                        };
                        dBEntities.Participants.Add(p);
                        tempEmails.Add(thisEmail);
                        MailAddress receiverAddress = new MailAddress(thisEmail);
                        string body = "You have been invited to the following discussion -<br/><br/><b style='font-size:1.5em'>Topic: "
                            + discussionTopic + "</b><br/><br/><b style='font-size:1em'>Description: " + discussionDescription
                            + "</b><br/><br/>Click on the following link to enter the discussion:<br/><a href='https://localhost:44315/Discussion/ParticipantRegistration?token=" + token + "'>" +
                            "https://localhost:44315/Discussion/ParticipantRegistration?token=" + token + "</a>";
                        using (
                MailMessage message = new MailMessage(senderAddress, receiverAddress)
                {
                    Subject = subject,
                    Body = body
                }
                    )
                        {
                            message.IsBodyHtml = true;
                            client.Send(message);
                        }
                    }
                }
            }
            dBEntities.SaveChanges();
            try
            {
                rngCryptoServiceProvider.Dispose();
            }
            catch (Exception) { }
            return RedirectToAction("ModeratorRoom");
        }

    }
}