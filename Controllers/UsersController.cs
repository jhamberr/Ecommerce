using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.Controllers
{
    //only accessible to admin so protect it using authorized attribute
    [Authorize(Roles = "admin")]//by default the different actions of this controller will be accessible at the url /users but becausue this controller is only available to admin we want to change that
    [Route("/Admin/[controller]/{action=Index}/{id?}")]//want to change url and add /admin to beginning using route attribute to define route pattern so the differnt actions of this controller will be accesible at this url starting with /admin then name of controller which is users then name of action and if none provided index executes then optional id parameter
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly int pageSize = 5;//display 5 users per page set to lower number to display pagination

        //request identity services from the service container. make constructor and add required services
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            this.userManager = userManager;
            this.roleManager = roleManager;
            //to read the list of registered users we only need the user manager service. we can request role manager because we weill use it later


        }

        public IActionResult Index(int? pageIndex)//this parameter comes from here--> asp-route-pageIndex="@i"
        {


            //create queryable variable of type application user to store returned users descending at created at
            IQueryable<ApplicationUser> query = userManager.Users.OrderByDescending(u => u.CreatedAt);

            /*less proficient way: read the registered users using userManager orderbydescending for newest users first
              var users = userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();*/

            //pagination
            if (pageIndex == null || pageIndex < 1)//if page index value invalid display first page
            {
                pageIndex = 1;
            }
            decimal count = query.Count(); //count the users
            int totalPages = (int)Math.Ceiling(count / pageSize);//users / pageSize = totalPages
            /*page = 2. 2-1 * pagesize(5) = skip the first five users and take pagesize(5) the next five. this is page 2
              skip the users of the previous pages and take the users of the requested page
              number of previous users = ((pageIndex-1) * pagesize) if page = 3 
            then 3-1 is 2 so 2 pages of prev users at 5 per page so  2 * page size*/
            query = query.Skip(((int)pageIndex - 1) * pageSize).Take(pageSize);

            //read the registered users using userManager orderbydescending for newest users first more proficient way
            var users = query.ToList();

            ViewBag.PageIndex = pageIndex;
            ViewBag.Totalpages = totalPages;

            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)//incorrect value of id
            {
                return RedirectToAction("Index", "Users");//redirect to  index action of user controller. redirect user to list of users
            }
            //otherwise read the user having the id passed in using usermanager service of type application user. service can manipulate the object type
            var appUser = await userManager.FindByIdAsync(id);

            if (appUser == null)//we did not find the user with this id so redirect to list of users
            {
                return RedirectToAction("Index", "Users");//redirect admin to list of users
            }
            /*otherwise read the found user's role. create a property in viewbag called Roles.
             * dynamically creating a property called Roles within ViewBag
             * read the different roles and save them in the roles property of viewbag*/
            ViewBag.Roles = await userManager.GetRolesAsync(appUser);

            //get available roles. make the select item viewbag variable here
            var availableRoles = roleManager.Roles.ToList();//use role manager to read the 3 defined roles and returns list of objects
            var items = new List<SelectListItem>();//use this to fill select element.list of selectlistitem objects
            foreach (var role in availableRoles) //for every role object in available roles
            {
                items.Add(/*add the role object to the item list of selectlistitem objects filling the fields of 
                           select list item object*/
                    new SelectListItem
                    {
                        Text = role.NormalizedName,//text visible to user
                        Value = role.Name,
                        /*select item in select list only if it corresponds to the current user.
                         if the user that we found (appUser) has the curr role we are iterating
                        over in the dictionary then it returns true below*/
                        Selected = await userManager.IsInRoleAsync(appUser, role.Name!),//current role will be shown on modal drop down list first
                    });
            }
            //after the items are populated then send to the view 
            ViewBag.SelectItems = items;

            return View(appUser);
        }

        public async Task<IActionResult> EditRole(string? id, string? newRole)
        {
            if (id == null || newRole == null) //if either are null return the user to list of users
            {
                return RedirectToAction("Index", "Users");
            }

            var roleExists = await roleManager.RoleExistsAsync(newRole);//true if role exists
            var appUser = await userManager.FindByIdAsync(id);//find user
            if (appUser == null || !roleExists) //if user is null or role doesnt exist return user to list of users
            {
                return RedirectToAction("Index", "Users");
            }

            var currentUser = await userManager.GetUserAsync(User);//get current user using User property of this controller
            //check if curr user which is Admin is == appuser which is passed in user we found. only admin can access details
            //wont allow admin to change his own role security purpose
            if (currentUser!.Id == appUser.Id)//check if appUser is the curr admin logged in so if you search for your own id then you cannot update yourself
            {
                TempData["ErrorMessage"] = "You cannot update your own role!";
                return RedirectToAction("Details", "Users", new { id });//redirect to details page with id
            }
            //othersise if admin is updating a user role then proceed
            var userRoles = await userManager.GetRolesAsync(appUser);//read the roles of the user
            await userManager.RemoveFromRolesAsync(appUser, userRoles);//delete all roles of this user
            await userManager.AddToRoleAsync(appUser, newRole);//add new role to user
            //if update was successful then add success message to temp data
            TempData["SuccessMessage"] = "User Role updated successfully";
            //after update redirect Admin to users page. 
            return RedirectToAction("Details", "Users", new { id });
        }


        public async Task<IActionResult> DeleteAccount(string? id)
        {
            if (id == null)//null value
            {
                return RedirectToAction("Index", "Users");
            }
            //otherwise read the user containing the id passed in
            var appUser = await userManager.FindByIdAsync(id);
            if (appUser == null) // didnt find user
            {
                return RedirectToAction("Index", "Users");
            }
            //check to make sure this isnt the current user. current user should not be able to delete thier account
            var currentUser = await userManager.GetUserAsync(User);//get user of the session
            if (currentUser!.Id == appUser.Id) //curr user is curr user in the session. app user is user we are searching
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction("Details", "Users", new { id });//redirect to details page of user having this id
            }
            //delete account
            var result = await userManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Users");//redirect admin to list of users
            }
            //else  if unsuccessful redirect to users page pertaining to the id of the user
            TempData["ErrorMessage"] = "Unable to delete this account: " + result.Errors.First().Description;
            return RedirectToAction("Details", "Users", new { id });
        }

    }
}
