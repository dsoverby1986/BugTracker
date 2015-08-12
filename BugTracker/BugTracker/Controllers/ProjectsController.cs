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

namespace BugTracker.Models
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); //Is 'db' a representation of the entire database?

        // GET: Projects
        public ActionResult Index()//in my current code in the route.config file the landing page is the projects index page...this method
        //is used to generate that page
        {
            if (User.Identity.IsAuthenticated)
            { 
                var user = db.Users.Find(User.Identity.GetUserId());
                var projectList = from project in db.Projects select project;//the variable 'projectList' is a list of all of the projects in the database
                //'from' declares an iteration variable called 'str' which will traverse the input collection like in a foreach loop.
                //This query returns a list of projects complete with all of their properties, like pulling all of the rows out of the 'Projects'
                //table one by one and putting them in a bag.  'projectList' is a collection of all of the projects from the 'Projects' table.
            
                return View(projectList.Where(p => p.Users.Any(u => u.Id == user.Id)).OrderByDescending(p => p.Created).ToList());//the method returns the Index.cshtml view with the collection of
                //projects sorted in descending order...sorting by the Created property 
            }
            else
            {
                return View();
            }
        }

        // GET: Projects/Details/5
        public async Task<ActionResult> Details(int? id)//'Details' action method called when the user wants to look at the details of a 
        {                                               //project...takes an integer called 'id' as input
            if (id == null)//if the input 'id' is null
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);//return a bad request notification
            }
            //otherwise...
            Project project = await db.Projects.FindAsync(id);//go to the 'Projects' table in the database and find the project with the 
            //id matching the id that was passed to this method and if a project does have an id that matches assign that project to a 
            //variable called 'project' that is of the type 'Project'...this variable has to be of the type 'Project' because I am 
            //expecting the project that is found in the database to have the same fields as the ones that are listed in the 'Projects' 
            //model class...is this all right?

            if (project == null)//if 'project' is null
            {
                return HttpNotFound();//return the httpnotfound message
            }
            return View(project);//otherwise, return the 'Details' view with the project that had an id that matches the id passed to this method
        }

        // GET: Projects/Create
        public ActionResult Create()//'Create' action method called when the user submits a request to the server that indicates that
                                    //the user want to create a new project...go to the 'Create' action in the 'Projects' controller
        {
            return View();//The 'Create' view in the 'Projects' folder is returrned
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]//indicates that this method only handles POST requests, POST methods submit data to be handled by a specified resource
        [ValidateAntiForgeryToken]//prevents fake post requests somehow
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,Created")] Project project)//The user has just filled 
        //out the form in the 'Create' view and has submitted the data to be processed by clicking the 'Create' button on the page.
        //That 'Create' button submits a URL request to the server which is processed and matched to a pattern in the route.config file. The 
        //request will match up with the 'Project' controller and the 'Create' action method.  Because the form that the user filled out has 
        //a 'method' attribute that has a vaule of 'post', the data submission request knows to come to the 'post' action method. The 'Bind' 
        //in the parameters area lays out how model binding should occur. In this case the 'Id', 'Name', 'Descripton' and 'Created' values 
        //are the data that gets mapped to the 'project' variable. Because the 'project' variable is of the type 'Project' and the 'Project'
        //model class has fields whose names correspond to the names of the data being passed to this method, those values can be mapped to 
        //the 'project' variable.  I don't remember what the 'async' and 'task' keywords are for.....does all of this sound right?
        {
            if (ModelState.IsValid)//I guess this is saying: if the data being passed to this method didn't have any problems binding with 
            //the 'project' parameter....do this stuff that comes next....is that right?
            {
                
                project.Created = DateTimeOffset.Now;//the 'Created' property doesn't have it's value specified by the user so here the value
                //is set to be the current date and time.
                var id = User.Identity.GetUserId();
                var user = db.Users.Find(id);
                project.Users.Add(user);

                db.Projects.Add(project);//go to the 'Projects' table in the database and add this 'project' with all of its properties and
                //their values.
                await db.SaveChangesAsync();//Again, I don't remember what the 'await' keyword means or does but this is a command to save
                //the changes made to the database.
                return RedirectToAction("Index");//This invokes the 'Index' action method, instead of returning a view. Go look at the 
                //'index' action method in this controller to see what happens next.
            }

            return View(project);//is something went wrong with the binding then the method returns the 'Create' view with the data that was
            //passed to it....'project'
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)//The user was looking at a project and wanted to edit it. They click the 'edit' link, which submits
        //a url request that is processed and then matched to a pattern in the route.config file. The request will match up with the 'Edit'
        //action method in the 'Projects' controller. This action method takes an integer as a parameter. That integer is named 'id'.
        {
            if (id == null)//if the 'id' passed to this action method is null...do this next stuff....
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);//return a 'bad request' message.
            }
            //otherwise...
            Project project = db.Projects.Find(id);//go to the 'Projects' table in the database and match the 'id' that was passed to this
            //method with one of the 'id's in the table and then assign that project to the variable 'project' of the type project. Because
            //'project' is of the type 'Project' all of the properties and their values can be mapped to the 'project' variable.
            if (project == null)//if 'project' is null, because the 'id' could not be matched to any of those listed in the 'Projects' table
            //of the database...do this next stuff...
            {
                return HttpNotFound();//return a httpnotfound message.
            }
            //otherwise...
            //generate a multiselect list that is popluated with all of the users
            //have the id of the users be the selectable property
            //have the display names of the users be what is displayed in the multiselect list
            //the selected users are the users that already have an association with this project
            ViewBag.UsersAssigned = new MultiSelectList(db.Users, "Id", "DisplayName", project.Users.Select(u=>u.Id));
            return View(project);//return the 'Edit' view with the 'project'...all of the data associated with the project that had the 'id'
            //that matched in input 'id'
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]//indicates that this method only handles POST requests, POST methods submit data to be handled by a specified resource
        [ValidateAntiForgeryToken]//prevents fake post requests somehow...right now i don't care how it does this...just that it does it
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Created,Updated")] Project project, string[] usersAssigned)
        //Once the user has
        //edited the project however they wished to, they will click the 'save' button and that button submits a url request that is
        //processed and then matched to a pattern in the route.config file. Once a pattern matches, the sumbission is routed to this 'Edit'
        //action method in the 'Projects' controller. This action method takes a 'project' of the type 'Project' as a parameter. The model
        //binding is specified and the data sumbitted is mapped to the 'project' variable...i still don't remember what the keywords 'await'
        //and 'task' are for or are doing.
        {
            if (ModelState.IsValid)//if there were no problems with the model binding...do the next stuff...
            {
                var dbProject = db.Projects.FirstOrDefault(p => p.Id == project.Id);

                if (dbProject == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                dbProject.Users.Clear();

                foreach (var userId in usersAssigned)
                {
                    var user = db.Users.Find(userId);
                    dbProject.Users.Add(user);
                }
                //db.SaveChanges();
                dbProject.Updated = DateTimeOffset.Now;//the user cannot specify the value of the 'Updated' property. Here the value of the
                //'Updated' property is set as thhe current date and time.
                dbProject.Name = project.Name;
                dbProject.Description = project.Description;

                await db.SaveChangesAsync();//any changes that were made to the database are saved...still don't remember what the 'await'
                //keyword is doing or means.
                return RedirectToAction("Index");//the method then calls the 'Index' method of the 'Projects' controller.
            }
            //var dbProject = db.Projects.FirstOrDefault(p => p.Id == project);

            return View(project);//if there was a problem with the model binding then return the 'Edit' view with the data that was passed
            //to this method and assigned to the 'project' parameter.
        }

        // GET: Projects/Delete/5
        public async Task<ActionResult> Delete(int? id)//The user has clicked on the 'Delete' link because they want to delete whatever
        //project they are looking at.  That link submits a url request that is processed and mapped to a url pattern in the route.config
        //file. When the url request matches one of the patterns in the route.config file, it is routed to the 'Delete' action method in
        //the 'Projects' controller. This action method takes an integer as input, calling it 'id'. I still don't know what the keywords
        //'async' and 'Task' mean or do.
        {
            if (id == null)//if the parameter 'id' is null...do the next stuff...
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);//returns a 'bad request' message
            }
            Project project = await db.Projects.FindAsync(id);//go to the 'Projects' table in the database with the 'id' that was passed
            //to this method and find an 'id' that matches it and then assign the project that matches that 'id' to the variable 'project'.
            //Because 'project' is of the type 'Project', all of the properties of the project that was matched in the database can be 
            //mapped to the 'project' variable...am i using the term mapped correctly or is there a better term to describe that replication
            //of values?
            if (project == null)//if no project in the 'Projects' table had a matching 'id'...do the next stuff...
            {
                return HttpNotFound();//return an httpnotfound message
            }
            //otherwise...
            return View(project);//return the 'Delete' view with the 'project' and all of it's properties to be viewed.
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]//not really sure what the deal is with this attribute pointing at the 'Delete' action method
        [ValidateAntiForgeryToken]//prevents fake post requests somehow...right now i don't care how it does this...just that it does it
        public async Task<ActionResult> DeleteConfirmed(int id)//The user wanted to delete a project. They clicked the 'Delete' button and
        //submitted a URL request that was then processed and matched to a pattern in the route.config file. Once the pattern was matched
        //the submission was directed to the 'DeleteConfirmed' action method in the 'Projects' controller. This method takes an integer as
        //a parameter. It is called 'id'. I still don't remember what 'async' or 'Task' mean or do.
        {
            Project project = await db.Projects.FindAsync(id);//go to the 'Projects' table in the database and match the value of the 'id'
            //parameter to one of the projects. Once a match is made, assign that project to the 'project' variable. Because the 'project'
            //variable is of the type 'Project', all of the property values of the project that has been selected in the database can be
            //mapped to the properties of the 'project' variable.
            db.Projects.Remove(project);//delete the 'project' from the 'Project' table in the database.
            await db.SaveChangesAsync();//save any changes made to the database.
            return RedirectToAction("Index");//this method redirects the request to the 'Index' action method of the 'Projects' controller.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /*
        private void SetProjectUsers(int projectId, string[] userIds)//call this function in the create and edit action methods to clear out the multiselect list of users and add them all back in that way i dont have to worry abouut who is in and how is out.....i can just select all of the users i want to be in a role for a particular project.
        {
            var project = db.Projects.Find(projectId);
            project.Users.Clear();//clear all users in the multiselect list
            foreach (var userId in userIds)//add all of the users back in.
            {
                project.Users.Add(db.Users.Find(userId));
            }
        }*/
    }
}
