using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

namespace Ecommerce.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 8;//display only 8 products per page

        public StoreController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = context.Products;//why not just use context as is?


            //search functionality
            if (search != null && search.Length > 0)
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            //filter functionality
            if (brand != null && brand.Length > 0)
            {
                query = query.Where(p => p.Brand.Contains(brand));
            }

            if (category != null && category.Length > 0)
            {
                query = query.Where(p => p.Category.Contains(category));
            }

            //Sort functionality
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);

            }
            else
            {
                //sort by newest products first
                query = query.OrderByDescending(p => p.Id);
            }

            //query = query.OrderByDescending(p => p.Id);

            // pagination functionality
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            //read products of requested page
            decimal count = query.Count();//get total number of products
            int totalPages = (int)Math.Ceiling(count / pageSize);//divide amount of products by page size to get total pages 20 count / 4 per page = 5 pages of 4
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize); /*tells how many pages to skip then multiply by iitems per page .
                                                                            * skip items then take the next in line after the skip*/
            /*pageindex-1 is amount of prev pages and multiply by page size we get number of products to skip
             take apge size means take only the allowed amount according to page size (8) and skipp the others before it*/

            //var products = context.Products.OrderByDescending(p=> p.Id).ToList(); deleted statement
            var products = query.ToList();// query is where we did the calculation too filter pages then we store in products 
            ViewBag.Products = products;/*put filtered products in query bag so we only show 8 per page. is the qyery.
           skip being done interactively? we are passing a dynamic query that sill execute ongoing?
            the pageindex is sent in and then everything is done here and sent to cshmtl and products
            are displayed 8 at a time according to current page and at the bottom is the paginization.
            the pagination is abstracted so not fully understanding how it all works? ask to hard code it?*/
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            //make an instantiation of class here
            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            return View(storeSearchModel);
        }

        public IActionResult Details(int id)
        {
            var product = context.Products.Find(id);
            if(product == null)
            {
                //redirect to list of producs
                return RedirectToAction("Index", "Store");
            }


            return View(product);
        }
    }
}
