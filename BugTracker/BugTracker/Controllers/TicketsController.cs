using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.IO;

namespace BugTracker.Models
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated) { 
                IEnumerable<Ticket> tickets;
                var user = db.Users.Find(User.Identity.GetUserId());

                if (User.IsInRole("Admin"))
                    tickets = db.Tickets;
                else if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                {
                    tickets = user.Projects.SelectMany(p => p.Tickets);
                }
                else if (User.IsInRole("Submitter"))
                {
                    tickets = db.Tickets.Where(t => t.OwnerUserId == user.Id);
                }
                else
                    tickets = new List<Ticket>();
            
                //var tickets = db.Tickets.Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.Type);
            
                return View(tickets.ToList());
            }
            else
            {
                return View();
            }
            //return View(await tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ticket = db.Tickets.Find(id);
                //.Include(t => t.Comments);
            //Ticket ticket = await db.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Route("Projects/{projectId}/Tickets/Create")]
        public ActionResult Create(int projectId)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            ViewBag.Title = "Create A New Ticket For '" + project.Name + "'";
            ViewBag.ProjectId = projectId;
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Projects/{projectId}/Tickets/Create")]
        public async Task<ActionResult> Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Ticket ticket, int? projectId)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = User.Identity.GetUserId();
                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }

            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Ticket ticket = db.Tickets.Find(id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "DisplayName", ticket.AssignedToUserId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Ticket ticket, string assignedUser)
        {
            var editable = new List<string>() { "Title", "Description" };
            if (User.IsInRole("Admin"))
                editable.AddRange(new string[] { "AssignedToUserId", "TicketTypeId", "TicketPriorityId", "TicketStatusId", "Updated"});
            if (User.IsInRole("Project Manager"))
                editable.AddRange(new string[] { "AssignedToUserId", "TicketTypeId", "TicketPriorityId", "TicketStatusId", "Updated" });

            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.Now;
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                var histories = GetTicketHistories(oldTicket, ticket).Where(h => editable.Contains(h.History.Property));

                var mailer = new EmailService();
                foreach (var item in histories)
                {
                    db.TicketHistories.Add(item.History);
                    if (item.Notification != null)
                       await mailer.SendAsync(item.Notification);
                }


                db.Update(ticket, editable.ToArray());
                /*
                if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                {
                    db.Update<Ticket>(dbTicket, "Title", "Description", "Created", "Updated", "ProjectId", "TicketTypeId", "TicketPriorityId", "TicketStatusId", "OwnerUserId", "AssignedToUserId");
                }
                else{
                    db.Update<Ticket>(dbTicket, "Title", "Description", "Created","Updated","ProjectId","TicketTypeId","TicketPriorityId","TicketStatusId","OwnerUserId");
                }*/
                //dbTicket.AssignedToUser.Clear();   //cannot call the .Clear() method on dsTicket.AssignedToUser for some reason
                //dbTicket.AssignedToUser.Add(assignedUser);//cannot call the .Add() method on dbTicket.AssignedToUser for some reason
                //i had to select 'generate method stub for both .Clear() and .Add()....what's up with that?

                //db.Entry(ticket).State = EntityState.Modified;
                
                await db.SaveChangesAsync();
                return RedirectToAction("Details", new { ticket.Id });
            }
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "DisplayName", ticket.AssignedToUserId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = await db.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Ticket ticket = await db.Tickets.FindAsync(id);
            db.Tickets.Remove(ticket);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST Tickets/Details/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateComment([Bind(Include = "TicketId,Comment,Created")]TicketComment ticketComment, int ticketId)
        {
            if (ModelState.IsValid)
            {
                ticketComment.UserId = User.Identity.GetUserId();
                ticketComment.Created = DateTimeOffset.Now;
                db.TicketComments.Add(ticketComment);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = ticketId });
            }
            return View(ticketComment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAttachment([Bind(Include = "TicketId,Description")]TicketAttachment ticketAttachment, HttpPostedFileBase file, int ticketId)
        {
            if (file != null && file.ContentLength > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();

                if (ext != ".png" && ext != ".jpg" && ext != ".gif" && ext != ".bmp" && ext != ".txt" && ext != ".pfd")
                {
                    ModelState.AddModelError("file", "Invalid Format.");
                }
            }

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var filePath = "/img/";
                    var absPath = Server.MapPath("~" + filePath);
                    ticketAttachment.FileUrl = filePath + file.FileName;
                    file.SaveAs(Path.Combine(absPath, file.FileName));
                }

                ticketAttachment.UserId = User.Identity.GetUserId();
                ticketAttachment.Created = DateTimeOffset.Now;
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = ticketId });
            }
            return View(ticketAttachment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<TicketHistoryWithNotification> GetTicketHistories(Ticket oldTicket, Ticket newTicket)
        {
            var histories = new List<TicketHistoryWithNotification>();
            var oldUser = db.Users.Find(oldTicket.AssignedToUserId);
            var newUser = db.Users.Find(newTicket.AssignedToUserId);

            if (oldTicket.AssignedToUserId != newTicket.AssignedToUserId)
            {
                histories.Add(new TicketHistoryWithNotification()
                {
                    History = new TicketHistory()
                    { 
                        TicketId = newTicket.Id,
                        Property = "AssignedToUserId",
                        PropertyDisplay = "Assigned User",
                        OldValue = oldTicket.AssignedToUserId,
                        OldValueDisplay = oldUser != null ? oldUser.DisplayName : "Unassigned",
                        NewValue = newTicket.AssignedToUserId,
                        NewValueDisplay = newUser != null ? newUser.DisplayName : "Unassigned",
                        Changed = DateTimeOffset.Now
                    },
                    Notification = newUser != null ? new IdentityMessage()
                    {
                        Subject = "You have been assigned to a ticket.",
                        Destination = newUser.Email,
                        Body = "You have been assigned to a ticket. The ticket is titled '" + newTicket.Title + "'."
                    } : null
                });
            }

            if (oldTicket.Description != newTicket.Description)
            {                  
                histories.Add(new TicketHistoryWithNotification()
                {
                    History = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        PropertyDisplay = "Description",
                        OldValue = oldTicket.Description,
                        OldValueDisplay = "'" + oldTicket.Description + "'",
                        NewValue = newTicket.Description,
                        NewValueDisplay = "'" + newTicket.Description + "'",
                        Changed = DateTimeOffset.Now
                    },
                    Notification = newUser != null ? new IdentityMessage()
                    {
                        Subject = "The description of your ticket, " + newTicket.Title + ", has changed.",
                        Destination = newUser.Email,
                        Body = "The descripton for your ticket, " + newTicket.Title + ", has changed. The new description is as follows: <br /><br /><br />" + newTicket.Description
                    } : null
                });
            }
            if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
            {
                var oldDisplay = oldTicket.Type != null ? oldTicket.Type.Name : "No Type";
                var newType = db.TicketTypes.Find(newTicket.TicketTypeId);
                var newDisplay = newType != null ? newType.Name : "No Type";

                histories.Add(new TicketHistoryWithNotification()
                    {
                        History = new TicketHistory()
                        {
                            TicketId = newTicket.Id,
                            Property = "TicketTypeId",
                            PropertyDisplay = "Ticket Type",
                            OldValue = oldTicket.TicketTypeId.ToString(),
                            OldValueDisplay = oldDisplay,
                            NewValue = newTicket.TicketTypeId.ToString(),
                            NewValueDisplay = newDisplay,
                            Changed = DateTimeOffset.Now
                        },
                        Notification = null
                    });
            }
            if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
            {
                var oldDisplay = oldTicket.Status != null ? oldTicket.Status.Name : "No Status";
                var newStatus = db.TicketStatuses.Find(newTicket.TicketStatusId);
                var newDisplay = newStatus != null ? newStatus.Name : "No Status";

                histories.Add(new TicketHistoryWithNotification()
                {
                    History = new TicketHistory()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketStatusId",
                        PropertyDisplay = "Ticket Status",
                        OldValue = oldTicket.TicketStatusId.ToString(),
                        OldValueDisplay = oldDisplay,
                        NewValue = newTicket.TicketStatusId.ToString(),
                        NewValueDisplay = newDisplay,
                        Changed = DateTimeOffset.Now
                    },
                    Notification = null
                });
            }

            return histories;
        }

        private class TicketHistoryWithNotification
        {
            public TicketHistory History { get; set; }
            public IdentityMessage Notification { get; set; }
        }
    }
}
