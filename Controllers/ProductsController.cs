using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{

    [Authorize(Roles = "admin")]//only allow users with role of admin to access entire controller
    //this is the controller of the administrator
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;//review
        private readonly int pageSize = 5;//how many products per  page

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        //page index will be the requested page "?" mean can be null
        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            //query product table --get the context of the curr table and store context in Iqueryable variable/list/iterable?
            IQueryable<Product> query = context.Products;
           
            //search functionality
            if (search != null)
            {
                //query each record such that name = search or brand = search
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            }
            //Sort Functionality
            string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };
            /* if val of col does not belong to this array then val of col is invalid
             default to "Id"?*/
            if (!validColumns.Contains(column))
            {
                column = "Id";//means by default order products by Id string
            }
            /* if val of orderBy does not belong to this array then val of orderBy is invalid
             so default to Desc*/
            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";//by default order by descending
            }

            if(column == "Name")
            {
                //if ascending order by ascending by name
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else 
                {
                    query = query.OrderByDescending(p => p.Name);
                }
            }
            else if (column == "Brand")
            {
                //if ascending order by ascending by Brand
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }
            else if (column == "Category")
            {
                //if ascending order by ascending by Category
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Category);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category);
                }
            }
            else if (column == "Price")
            {
               
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }
            else if (column == "CreatedAt")
            {

                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else //otherwise default sort by ID
            {

                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }


            //pagination
            //check if page index is valid
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            /*find total number of pages*/
            decimal count = query.Count();// find total number of products
            int totalPages = (int)Math.Ceiling(count / pageSize);/*calculate total number of pages
                                                                  ceiling converts 4.5 to 5 if thats the case
            total pages allows us to display the pagination buttons*/
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);/*read products of requested page
                      skip products of prev pages */

            var products = query.ToList();   //read the 5 products in list form and pass to a variable
            /*need to pass total pages to view to display pagination buttons and need
         to pass curr page index to view to highlight the button of curr page*/
            /*view data dictionary is data interpolation like in python that 
             * has a data object when forms are submitted to the backend
             viewdata is a built in data structure that can be used to send information to the view cshtml file*/
            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            //pass the search query parameter to the view by way of storing it in view data object
            //if search not null save to view. teriary comparison
            //if null save empty string
            ViewData["Search"] = search ?? "";
            /* add val of col and orderBy to view data dictionary so we can access them from the view*/
            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy; 
            //pass list of products to view
            return View(products);
        }

        public IActionResult Create()
        { 
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            //validate image file
            if (productDto.ImageFile == null)
            {
                //error bound to imagefile property type
                ModelState.AddModelError("ImageFile", "The image file is required");
            }
            // check if there is any validation error in productDTO
            // if it isnt valid then return the create view with the submitted data again
            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            //save image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            //save the new product to the database
            //use productDTO from the create page to store the information in a product isntance
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            //add the product to the Database
            context.Products.Add(product);
            //save changes to the DB/save modifications
            context.SaveChanges();
            //or redirect user to list of products
            //redirect to index action of products controller
            return RedirectToAction("Index", "Products");
        }

        //action takes product ID.read that id from the database and use db context to read product
        public IActionResult Edit(int id)
        {
            /*product is auto casted as product so will be auto assigned the 
             * null value if nothing is found and we redirect to list of products 
             or in other words the index*/
            var product = context.Products.Find(id);
            if (product == null)
            {
                /*Action is Index and controlelr is products*/
                return RedirectToAction("Index", "Products");
            }
            /*If we found a product then we will create an object of type product DTO using the data of
              this product*/
            /*use condensed form of the product class to make a product out of the id found 
             * in the DB/Edit the ID
             fill data in with product object that we obtained from the DB then return a view with this object*/
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,

            };
            //cant access id directly so add to dictionary called view data built in datatype
            //key value pair to viewdata containder dictionary?
            //why couldnt we access product ID in the view instead of dicitonary?
            ViewData["ProductId"] = product.Id;
            ViewData["ImageFilename"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
            /*return a view and provide the view with this object then create the view
             call it edit.cshtml and it will be in products folder under views*/
            return View(productDto);
        }
        //Action to update. Modified edit accessible using post
        //how is the submit button linked to post? it sends post json?
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            //read product from database using context
            var product = context.Products.Find(id);
            //redirect if you didnt find the product
            if (product == null)
            {
                return RedirectToAction("Index", "Products");

            }
            //check if submitted data is valid 
            //if not valid means we have some validation error
            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFilename"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                //return back to edit view providing it with produectDTO object
                return View(productDto);
            }
            //update image file if we have new image file
            //where is the new image being retrieved?
            //initialize with old filename below
            string newFileName = product.ImageFileName;
            //if we have the file then update filename
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                //create stream and save new file to stream
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
                //delete old image
                string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            //update product in DB
            //update all of the fields from the submitted data available in product DTO
            /*product is the product that we received from the DB above with the context and we store
             new info submitted from the form and stored in product DTO data transfer happening here.*/
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            //update in database
            context.SaveChanges();
            //redirect user to list of products after save
            return RedirectToAction("Index", "Products");
        }
        public IActionResult Delete(int id)
        {
            //read product from database using context
            var product = context.Products.Find(id);
            //redirect if you didnt find the product
            if (product == null)
            {
                return RedirectToAction("Index", "Products");

            }
            //seperate statements to deal with deleting the image once product is retrieved from DB
            string ImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(ImageFullPath);
            //delete product from Database
            context.Products.Remove(product);
            context.SaveChanges(true);
            //redirect after changes saved and delete occured
            return RedirectToAction("Index", "Products");
        }
    }
}
