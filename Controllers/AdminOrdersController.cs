using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{

    //only available to authenticated usersth the role admin the different actions of this controller with be accessible at the url starting with /adminorders
    [Authorize(Roles ="admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]//this changes route pattern from above so this is the new pattern to access the actions of the controller^. by default the action is index and optional id parameter
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 5;//display 5 orders per page

        public AdminOrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }
        //read created orders
        public IActionResult Index(int pageIndex)
        {

            IQueryable<Order> query = context.Orders.Include(o => o.Client)
                .Include(o => o.Items).OrderByDescending(o => o.Id);
            
            //check if page index is valid
            if (pageIndex <= 0) 
            {
                pageIndex = 1;
            }
            //read the number of orders
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);//cailing includes the extra page for instances of 1.5 pages.
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var orders = query.ToList();//descending order by id or date for newest orders
            //pass orders to view by viewbag or model
            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        public IActionResult Details(int id)
        {
            var order = context.Orders.Include(o => o.Client).Include(o => o.Items)
                .ThenInclude(oi => oi.Product).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.NumOrders = context.Orders.Where(o => o.ClientId == order.ClientId).Count();

            return View(order);
        }
    }
}
