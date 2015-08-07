using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.ViewModels
{
    public class UserListViewModel
    {
        public IList<ApplicationUser> Users { get; set; }//an instance of this model class can have a list of users...is that right?
        public Dictionary<string, string> Roles { get; set; }//an instance of this model class can have a dictionary of roles(?) where
        //...i still don't really understand this dictionary...or these parameters
    }
}