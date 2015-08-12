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
using BugTracker.Models;
using BugTracker.Helpers;
using BugTracker.ViewModels;


namespace BugTracker.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()//url request is submitted, processed, matched to a pattern in the route.config file and the 'Index'
        //action method in the Admin controller is called. This action method takes no parameters. 
        {
            
            ViewBag.Title = "Users";

            var theViewModel = new UserListViewModel()//a new instance of the UserListViewModel is created and called 'theViewModel'
            {//this is a constructor that is going to give value to the properties of 'theViewModel'...is that right?
                Users = db.Users.ToList(),//create a list of the Users in the database and call that list 'Users'
                //get a dictionary of roles with key = id and value = name,
                //so we can easily lookup the role names given user.Roles...RoleId
                Roles = db.Roles.ToDictionary( k=> k.Id, v => v.Name)//go to the 'Roles' table in the database and create a dictionary
                //where the keys are the role id-s and the values are the role names and call this dictionary 'Roles'.
            };

            return View(theViewModel);//return the 'Index' view from the 'Admin' folder, with 'theViewModel' to work with.
        }

        //GET: Admin/ManageRoles/(role)
        public ActionResult ManageRoles(string role)//When the administrator wants to add many users to a role at once they will click
            //the button that indicates which role they want to add users to. that button submits a url request that is processed and 
            //matched to a pattern in the route.config file, which will direct to the 'ManageRoles' action method in the 'admin' controller.
            //the 'string role' parameter of this method receives the parameter indicated in the button that was clicked. The parameter will
            //be either 'admin', 'project manager', 'developer' or 'submitter'.
        {
            var helper = new UserRolesHelper();//this creates a new instance of the 'UserRolesHelper' class. the 'helper' object has
            //all of the methods defined in the 'UserRolesHelper' class.
            ViewBag.AssignedUsers = new MultiSelectList(db.Users.ToList(),"Id","DisplayName", helper.UsersInRole(role).Select(u => u.Id));
            //the 'AssignedUsers' variable is a new instance of the 'MultiSelectList' class and is attached to the 'ViewBag' so that a
            //multiselect list can be created in the view.  The parameters given to the 'MultiSelectList' object are 1.) a list of all users
            //in the database, 2.) the property that is going to be selectable in the list (the users Id), 3.) the property that will be displayed
            //in the multiselect list (the users DisplayName), 4.)...the 'UsersInRole' method is called from the 'helper' object and passed the 
            //'role' parameter that was passed to the 'ManageRoles' action method. From the 'UsersInRole' method is returned a list of users
            //whose 'RoleId' value matches the 'Id' value for that role in the 'Roles' table. So, this multiselect list will be populated with 
            //all of the Users in the database and the end user will see the user's display names and those display names will have the users' 
            //id's tied to them and any user that the end user selects on that multiselect list will be added to the role specified.
            return View();//return the ManageRoles view.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRoles(string role, string[] assignedUsers)//postback method will receive the role name given to the multiselect
            //list in the GET action method and it will receive a string array of the users that were selected in the multiselect list.
        {
            var helper = new UserRolesHelper();//a new 'UserRolesHelper' object is instantiated and named 'helper'. It has all of the methods
            //defined in the 'UserRolesHelper' class.
            var dbRole = db.Roles.FirstOrDefault(r => r.Name == role);//go to the 'Roles' table in the database and find the first role where
            //it's 'Name' property has the same value as the 'role' parameter that was passed to this action method and assign that role
            //(it's 'Name' and 'Id' properties and their values) to the variable called 'dbRole'.

            if (dbRole == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            dbRole.Users.Clear();//remove all of the users from the 'Users' property of 'dbRole'
            db.SaveChanges();//save any changes made to the database.

            foreach (var user in assignedUsers ?? new string[0])//loop over the string array 'assignedUser', which was passed as a parameter
                //to this acton method....LOOK INTO THE SECOND PART OF THIS LOOP STATEMENT...NOT REALLY SURE WHAT THAT IS ABOUT...
                helper.AddUserToRole(user, role);//call the 'AddUserToRole' method from the 'helper' object and pass it the current user from
            //the 'assignedUsers' array and the specified 'role'.

            return RedirectToAction("ManageRoles", new { role });//redirect to the 'ManageRoles' action method with a new object of the
            //'role' that was passed to this action method.
        }

        //GET: Admin/UserRoles/Id
        public ActionResult EditUserRoles (string id)//need to bring in the users id so that the user can be looked up in the userroles table
            //not sure 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser user = db.Users.Find(id);//not sure how to get a single user

            var roles = db.Roles.ToList();
            var selectedRoles = from userRole in user.Roles//for every role that the user has
                                select (
                                    from r in roles//for every role in the 'roles' list
                                    where r.Id == userRole.RoleId//get the role where the 'Id' has the same value as the current role from
                                    //'userRole'
                                    select r.Name//and get the value of the 'Name' property associated with that role
                                ).FirstOrDefault();//NOT SURE WHY I AM CALLING THE 'FirstOrDefault' METHOD ON THE RETURN SEQUENCE HERE.

            ViewBag.SelectedRoles = new MultiSelectList(roles, "Name", "Name", selectedRoles);

            return View(user);
        }

        //POST
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserRoles(string id, string[] roles)
        {
            return
        }*/
    }
}