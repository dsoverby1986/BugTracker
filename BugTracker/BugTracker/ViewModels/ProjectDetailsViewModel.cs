using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public IList<ApplicationUser> Users { get; set; }
    }
}