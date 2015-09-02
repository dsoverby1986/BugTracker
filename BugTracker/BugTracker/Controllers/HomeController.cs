using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using BugTracker.ViewModels;
using System.Configuration;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated) { 

                var user = db.Users.Find(User.Identity.GetUserId());

                var projectList = from project in db.Projects select project;

                var theViewModel = new HomePageViewModel();

                /*
                if (User.IsInRole("Admin"))
                {
                    ViewBag.TicketTotal = db.Tickets.Count();
                }
                else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                {
                    ViewBag.TicketTotal = user.Projects.SelectMany(p => p.Tickets).Count();
                }
                else if (User.IsInRole("Submitter"))
                {
                    ViewBag.TicketTotal = db.Tickets.Where(t => t.OwnerUserId == user.Id).Count();
                }
                */
                ViewBag.DisplayName = "Welcome, " + user.DisplayName;
                ViewBag.ProjectTotal = user.Projects.Count();
                ViewBag.NotificationTotal = user.Notifications.Count();
                ViewBag.UnassignedTickets = db.Tickets.Where(t => t.AssignedToUser == null).Count();
                
                if (User.IsInRole("Admin"))
                {
                    
                    ViewBag.TicketTotal = db.Tickets.Count();
                    ViewBag.CommentTotal = db.TicketComments.Count();
                    ViewBag.AttachmentTotal = db.TicketAttachments.Count();
                    //ViewBag.NotificationTotal = db.TicketNotifications.Where(n => n.UserId == user.Id).Count();
                    theViewModel = new HomePageViewModel()
                    {
                        Projects = projectList.Where(p => p.Users.Any(u => u.Id == user.Id)).OrderByDescending(p => p.Created).ToList(),
                        Tickets = db.Tickets.ToList()
                    };
                }
                if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                {
                    ViewBag.TicketTotal = user.Projects.Where(p => p.Tickets.Count > 0).SelectMany(p => p.Tickets).Count();
                    ViewBag.CommentTotal = user.Projects.Where(p => p.Tickets.Count > 0).SelectMany(p => p.Tickets).SelectMany(p => p.Comments).Count();
                    ViewBag.AttachmentTotal = user.Projects.Where(p => p.Tickets.Count > 0).SelectMany(p => p.Tickets).SelectMany(p => p.Attachments).Count();
                    
                    theViewModel = new HomePageViewModel()
                    {
                        Projects = projectList.Where(p => p.Users.Any(u => u.Id == user.Id)).OrderByDescending(p => p.Created).ToList(),
                        Tickets = user.Projects.SelectMany(p => p.Tickets).ToList()
                    };
                }
                if (User.IsInRole("Submitter"))
                {
                    ViewBag.TicketTotal = db.Tickets.Where(t => t.OwnerUserId == user.Id).Count();
                    ViewBag.CommentTotal = db.Tickets.Where(t => t.OwnerUserId == user.Id).SelectMany(t => t.Comments).Count();
                    ViewBag.AttachmentTotal = db.Tickets.Where(t => t.OwnerUserId == user.Id).SelectMany(t => t.Attachments).Count();
                    theViewModel = new HomePageViewModel()
                    {
                        Projects = projectList.Where(p => p.Users.Any(u => u.Id == user.Id)).OrderByDescending(p => p.Created).ToList(),
                        Tickets = db.Tickets.Where(t => t.OwnerUserId == user.Id).ToList()
                    };
                }

                return View(theViewModel);
            }
            else
            {
                ViewBag.DisplayName = "Welcome";
                ViewBag.ProjectTotal = 0;

                return View();
            }
            //NEED TO WORK OUT HOW TO GET THIS VIEW MODEL TO WORK FOR THE HOME PAGE
            /*if (User.IsInRole("Admin")) 
            { 
                
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                
            }
            else if(User.IsInRole("Submitter"))
            {
                
            }*/
            /*else
            {
                theViewModel = new HomePageViewModel()
                {
                    Projects = projectList.Where(p => p.Users.Any(u => u.Id == user.Id)).OrderByDescending(p => p.Created).ToList(),
                    Tickets = new List<Ticket>()
                };
            }*/
            //ViewBag.CommentTotal = user.Tickets.Where(t => t.OwnerUserId == user.Id).Select(t => t.Comments).Count();

            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Administrator";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactMessage form)
        {
            if (!ModelState.IsValid)
            {
                return View(form);
            }

            var emailer = new EmailService();

            var mail = new IdentityMessage()
            {
                Destination = ConfigurationManager.AppSettings["PersonalEmail"],
                Subject = form.Subject,
                Body = "You have received a new contact form submission from" + form.Name + "(" + form.FromEmail + ") with the following contents:<br /><br /><br />" + form.Message
            };

            emailer.SendAsync(mail);

            //TempData["MessageSent"] = "Your message has been delivered successfully.";
            ViewBag.Messagesent = "Your message has been delivered successfully.";
            return View();
            //return RedirectToAction("Contact");

            /* ViewBag.Message = "Contact Shane Overby";

             return View();*/
        }

        public ActionResult YouNeedToLogin()
        {
            return View();
        }
    }
}