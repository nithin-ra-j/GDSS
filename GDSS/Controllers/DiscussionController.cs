using GDSS.Handlers;
using GDSS.Models;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.EventArgs;

namespace GDSS.Controllers
{
    public class DiscussionController : Controller
    {
        GDSS_DBEntities dBEntities = new GDSS_DBEntities();

        // GET: Discussion
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult AddPoll(string Question = null, string optionA = null, string optionB = null, string Error = null)
        {
            ViewBag.Error = Error;
            ViewBag.Question = Question;
            ViewBag.optionA = optionA;
            ViewBag.optionB = optionB;
            return View();
        }

        [HttpPost]
        public ActionResult SavePoll(string Question, string optionA, string optionB, string[] options)
        {
            if (Question == "" || optionA == "" || optionB == "")
            {
                return RedirectToAction("AddPoll", new { Question, optionA, optionB, Error = "*Please fill all the required fields." });
            }
            int discussionID = (int)Session["Discussion"];
            Poll p = new Poll()
            {
                Question = Question,
                DiscussionID = discussionID,
                Status = "Saved",
                CreatedOn = DateTime.Now
            };

            dBEntities.Polls.Add(p);


            PollOption po = new PollOption()
            {
                Option = optionA
            };

            dBEntities.PollOptions.Add(po);

            po = new PollOption()
            {
                Option = optionB
            };

            dBEntities.PollOptions.Add(po);

            if (options != null)
                for (int i = 0; i < options.Length; i++)
                {
                    po = new PollOption()
                    {
                        Option = options[i]
                    };
                    dBEntities.PollOptions.Add(po);
                }
            dBEntities.SaveChanges();
            return RedirectToAction("ModeratorRoom", "Moderator");
        }

        public ActionResult ViewPoll()
        {
            int discussionID = (int)Session["Discussion"];
            int userID = (int)Session["User"];
            Poll p = dBEntities.Polls.Where(x => x.DiscussionID == discussionID && x.Status == "Active").ToList().LastOrDefault();
            if (p != null)
            {
                ViewBag.Question = p.Question;
                ViewBag.NoOfVotes = p.NoOfVotes;
                List<PollOption> po = new List<PollOption>(dBEntities.PollOptions.Where(x => x.PollID == p.Id));
                ViewBag.Options = po.ToArray();
                PollVote pv = dBEntities.PollVotes.FirstOrDefault(x => x.ParticipantID == userID && x.PollID == p.Id);
                ViewBag.PollVote = pv;
            }
            return View();
        }

        [HttpPost]
        public void RegisterVote(int id)
        {
            int discussionID = (int)Session["Discussion"];
            int userID = (int)Session["User"];
            dBEntities.RegisterVote(id, userID);
        }

        public ActionResult ViewForum()
        {
            int discussionID = (int)Session["Discussion"];
            Forum forum = dBEntities.Fora.FirstOrDefault(x => x.DiscussionID == discussionID && x.Status == "Active");
            if (forum != null)
            {
                ViewBag.Id = forum.Id;
                ViewBag.ForumTitle = forum.Title;
                ViewBag.Content = forum.Content;
                IQueryable<ForumComment> query = dBEntities.ForumComments.Where(x => x.ForumID == forum.Id);
                if (query != null)
                {
                    List<ForumComment> comments = query.ToList();
                    Dictionary<ForumComment, List<ForumReply>> forumDictionary = new Dictionary<ForumComment, List<ForumReply>>();
                    foreach (ForumComment comment in comments)
                    {
                        IQueryable<ForumReply> query2 = dBEntities.ForumReplies.Where(x => x.ForumCommentID == comment.Id);
                        List<ForumReply> replies = new List<ForumReply>();
                        if (query2 != null)
                            replies = query2.ToList();
                        forumDictionary.Add(comment, replies);
                    }
                    ViewBag.CommentReplies = forumDictionary;
                }
            }
            return View();
        }

        public void SaveForumComment(int forumID, string comment)
        {
            int participantID = (int)Session["User"];
            ForumComment fc = new ForumComment()
            {
                Comment = comment,
                ForumID = forumID,
                ParticipantID = participantID,
                PostedOn = DateTime.Now
            };
            dBEntities.ForumComments.Add(fc);
            dBEntities.SaveChanges();
        }

        public void SaveCommentReply(int commentID, string reply)
        {
            int participantID = (int)Session["User"];
            ForumReply fr = new ForumReply()
            {
                Reply = reply,
                ForumCommentID = commentID,
                ParticipantID = participantID,
                PostedOn = DateTime.Now
            };
            dBEntities.ForumReplies.Add(fr);
            dBEntities.SaveChanges();
        }

        public ActionResult AddForum()
        {
            return View();
        }

        public ActionResult SaveForum(string heading, string content)
        {
            int discussionID = (int)Session["Discussion"];
            @Forum f = new Forum()
            {
                Title = heading,
                Content = content,
                DiscussionID = discussionID,
                Status = "Saved",
                CreatedOn = DateTime.Now
            };

            dBEntities.Fora.Add(f);
            dBEntities.SaveChanges();
            return RedirectToAction("ModeratorRoom", "Moderator");
        }

        public ActionResult ParticipantRegistration(string token, string error = null)
        {
            if (Session["Role"] != null && Session["Role"].Equals("M"))
                return RedirectToAction("DiscussionMessage");
            Participant p = dBEntities.Participants.First(x => x.Token == token);
            if (p.Username == null)
            {
                string name = p.Name;
                TempData["Token"] = token;
                ViewBag.Token = token;
                ViewBag.Name = name;
                ViewBag.Error = error;
                return View();
            }
            else
            {
                Session["User"] = p.Id;
                Session["Role"] = "P";
                Session["Discussion"] = p.DiscussionID;
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
                return RedirectToAction("DiscussionRoom");
            }
        }

        public ContentResult GenerateUsername(string token)
        {
            string username;
            List<string> adjectives = dBEntities.Usernames.Select(x => x.Adjective).ToList();
            List<string> fishes = dBEntities.Usernames.Select(x => x.Fish).ToList();
            int discussionID = dBEntities.Participants.First(x => x.Token == token).DiscussionID;
            Random random = new Random();
            int index;
            string adjective, fish;
            do
            {
                index = random.Next(adjectives.Count);
                adjective = adjectives.ElementAt(index);
                index = random.Next(fishes.Count);
                fish = fishes.ElementAt(index);
                username = adjective + fish;
            } while (dBEntities.Participants.FirstOrDefault(x => x.DiscussionID == discussionID && x.Username == username) != null);
            return Content(username);
        }

        public ActionResult RegisterParticipant(string username)
        {
            string token = (string)TempData["Token"];
            int result = dBEntities.RegisterParticipant(token, username);
            if (result == 1)
            {
                Participant p = dBEntities.Participants.First(x => x.Token == token);
                Session["User"] = p.Id;
                Session["Role"] = "P";
                Session["Discussion"] = p.DiscussionID;
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
                return RedirectToAction("DiscussionRoom");
            }
            else return RedirectToAction("ParticipantRegistration", new { token, error = "The username has been taken. Please try again." });
        }

        public ActionResult DiscussionRoom()
        {
            int discussionID = (int)Session["Discussion"];
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            if (!d.Status.Equals("Active"))
                return RedirectToAction("DiscussionMessage");
            Poll p = dBEntities.Polls.Where(x => x.DiscussionID == discussionID && x.Status == "Active").ToList().LastOrDefault();
            if (p != null)
            {
                ViewBag.Poll = "true";
                ViewBag.Forum = "null";
            }
            else
            {
                Forum f = dBEntities.Fora.Where(x => x.DiscussionID == discussionID && x.Status == "Active").ToList().LastOrDefault();
                if (f != null)
                {
                    ViewBag.Poll = "null";
                    ViewBag.Forum = "true";
                }
                else
                {
                    ViewBag.Poll = "null";
                    ViewBag.Forum = "null";
                }
            }
            new SQLTableDependencyTriggers();
            return View();
        }

        public ActionResult DiscussionMessage()
        {
            if (Session["Role"] != null && Session["Role"].Equals("M"))
            {
                ViewBag.Message = "You are currently logged into a moderator account. Please use your dashboard to get into the discussion.";
                return View();
            }
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            if (d == null)
                ViewBag.Message = "This discussion has expired or been ended.";
            else
            {
                string status = d.Status;
                if (status.Equals("Upcoming"))
                    ViewBag.Message = "This discussion has not started yet. Please check back later or contact moderator.";
                else if (status.Equals("Paused"))
                    ViewBag.Message = "This discussion is paused. Please check back later or contact moderator.";
                else if (status.Equals("Ended"))
                    ViewBag.Message = "This discussion has ended.";
            }
            return View();
        }

        [HttpPost]
        public ContentResult TogglePause()
        {
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            if (d.Status.Equals("Active"))
            {
                d.PauseDateTime = DateTime.Now;
                d.Status = "Paused";
                dBEntities.SaveChanges();
                return Content("Discussion paused.");
            }
            else if (d.Status.Equals("Paused"))
            {
                d.DeletionDateTime = d.DeletionDateTime.Add(DateTime.Now.Subtract((DateTime)d.PauseDateTime));
                d.PauseDateTime = null;
                d.Status = "Active";
                dBEntities.SaveChanges();
                return Content("Discussion resumed");
            }
            else return Content("An error occured.");
        }

        [HttpPost]
        public ContentResult EndDiscussion()
        {
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            d.Status = "Ended";
            dBEntities.SaveChanges();
            return Content("Discussion ended.");
        }

        public ActionResult EditDiscussion()
        {
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            ViewBag.Topic = d.Topic;
            ViewBag.Description = d.Description;
            ViewBag.ClientName = d.ClientName;
            ViewBag.ClientEmail = d.ClientEmail;
            ViewBag.StartDateTime = d.StartDateTime.ToString("yyyy-MM-ddThh:mm");
            ViewBag.DeletionDateTime = d.DeletionDateTime.ToString("yyyy-MM-ddThh:mm");
            ViewBag.Status = d.Status;
            return View();
        }

        [HttpPost]
        public ActionResult PerformEditDiscussion([Bind(Include = "Topic,  Description,  ClientName,  ClientEmail, StartDateTime,  DeletionDateTime")] Discussion discussion)
        {
            Discussion d = dBEntities.Discussions.Find(Session["Discussion"]);
            if (d.Status.Equals("Paused") && d.DeletionDateTime != discussion.DeletionDateTime)
                d.PauseDateTime = DateTime.Now;
            d.Topic = discussion.Topic;
            d.Description = discussion.Description;
            d.ClientName = discussion.ClientName;
            d.ClientEmail = discussion.ClientEmail;
            if (d.Status.Equals("Upcoming"))
                d.StartDateTime = discussion.StartDateTime;
            d.DeletionDateTime = discussion.DeletionDateTime;
            dBEntities.SaveChanges();
            return RedirectToAction("ModeratorRoom", "Moderator");
        }

        public ActionResult WhiteBoard()
        {
            return View();
        }

        public class dash
        {

            public IEnumerable<Participant> Participants { get; set; }


        }

        public ActionResult Dashboard()
        {
            int discussionID = (int)Session["Discussion"];
            var tables = new dash
            {
                Participants = dBEntities.Participants.Where(x => x.DiscussionID == discussionID).ToList()
            };
            //var lastrow = dBEntities.Moderators.OrderByDescending(x => x.firstname.Length).ThenByDescending(x => x.firstname).FirstOrDefault().firstname;
            string firstname = dBEntities.Moderators.Find(Session["User"]).firstname;
            string lastname = dBEntities.Moderators.Find(Session["User"]).lastname;
            ViewBag.Moderator = firstname + ' ' + lastname;
            return View(tables);
        }

        public ActionResult ToolHub()
        {

            IQueryable<Poll> polls = dBEntities.Polls.Where(x => !x.Status.Equals("Ended"));
            if (polls != null)
            {
                ViewBag.Polls = polls.ToArray();
                foreach (Poll p in polls)
                    if (p.Status.Equals("Active"))
                    {
                        ViewBag.NoPush = true;
                        break;
                    }
            }
            IQueryable<Forum> forums = dBEntities.Fora.Where(x => !x.Status.Equals("Ended"));
            if (forums != null)
            {
                ViewBag.Forums = forums.ToArray();
                foreach (Forum f in forums)
                    if (f.Status.Equals("Active"))
                    {
                        ViewBag.NoPush = true;
                        break;
                    }
            }
            return View();
        }

        public void ChangePollState(string op, int pollID)
        {
            Poll p = dBEntities.Polls.Find(pollID);
            switch (op)
            {
                case "push":
                    p.Status = "Active";
                    p.PostedOn = DateTime.Now;
                    break;
                case "delete":
                    List<PollOption> po = dBEntities.PollOptions.Where(x => x.PollID == pollID).ToList();
                    List<PollVote> pv = dBEntities.PollVotes.Where(x => x.PollID == pollID).ToList();
                    dBEntities.PollVotes.RemoveRange(pv);
                    dBEntities.PollOptions.RemoveRange(po);
                    dBEntities.Polls.Remove(p);
                    break;
                case "end":
                    p.Status = "Ended";
                    break;
            }
            dBEntities.SaveChanges();
        }

        public void ChangeForumState(string op, int forumID)
        {
            Forum f = dBEntities.Fora.Find(forumID);
            switch (op)
            {
                case "push":
                    f.Status = "Active";
                    f.PostedOn = DateTime.Now;
                    break;
                case "delete":
                    dBEntities.Fora.Remove(f);
                    break;
                case "end":
                    f.Status = "Ended";
                    break;
            }
            dBEntities.SaveChanges();
        }

        public Forum Forum
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

        public Discussion Discussion
        {
            get => default;
            set
            {
            }
        }

        public Username Username
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
    }
}