using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using BugTracker.Models;

namespace BugTracker.Helpers
{
    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(
                    new ApplicationDbContext()));

        public bool IsUserInRole(string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public IList<string> ListUserRoles(string userId)
        {
            return manager.GetRoles(userId);
        }

        public bool AddUserToRole(string userId, string roleName)//'AddUserToRole' method takes two arguments as parameters, a userId and
            //a roleName. This method returns a boolean value.
        {
            var result = manager.AddToRole(userId, roleName);//call the 'AddToRole' method of the 'manager' object and pass it the 'userId'
            //and the 'roleName'. This method will return a boolean value. 'True' if the user was successfully add to the role.
            return result.Succeeded;//return the valueof the 'Succeeded' property of the 'result' variable.
        }

        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public IList<ApplicationUser> UsersInRole(string roleName)
        {
            var db = new ApplicationDbContext();//'db' is an instance of the entire database

            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);//go to the 'Roles' table in the database and get the first
            //instance where the Name property of the role has the value of the roleName passed to this method, returning all of the
            //properties and their values(properties being Name and Id in this case).

            if (role == null)
                return new List<ApplicationUser>();

            return db.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id)).ToList();//go to the 'Users' table in the database and
            //if a user has any roles that have a RoleId value the same as the role.Id value from the 'role' variable, add that user to 
            //the return sequence. Once the query is complete, the 'ToList()' method is called on the return sequence and that list
            //of users is returned. The 'ToList()' method prevents lazy execution of the query.
        }

        public IList<ApplicationUser> UserNotInRole(string roleName)
        {
            var db = new ApplicationDbContext();

            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);

            if (role == null)
                return new List<ApplicationUser>();

            return db.Users.Where(u => !u.Roles.Any(r => r.RoleId == role.Id)).ToList();
        }

        
    }
}