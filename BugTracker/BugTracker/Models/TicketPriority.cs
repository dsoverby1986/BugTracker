using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}