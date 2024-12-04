using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly decimal shippingFee;

        /*create constructor that allows us to request some services from the services container
ApplicationDbContext - allows us to save the orders in the database
IConfiguration -  allows us to read the configuration file Appsettings.json
UserManager<ApplicationUser> - we need user manager that allows us to read the details of the authenticated user*/
        public CartController(ApplicationDbContext context, IConfiguration configuration
            , UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
            shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee");
        }
        /*read shopping cart cookie and convert into list of order item objects*/
        public IActionResult Index()
        {
            //read list of order item objects
            //request contains the cookie, response that allows us to delete the cookie, context allows us to read product details from DB
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);//pass in the list obtained from GetCartItems
            //pass data to the view using viewbag
            ViewBag.CartItems = cartItems;
            ViewBag.ShippingFee = shippingFee; //from config
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal + shippingFee;

            return View();
        }

        //handles the post to the index. make sure that the user is authenticated. decorate with authorize attirubte
        [Authorize]
        [HttpPost]
        public IActionResult Index(CheckoutDto model )//requires submitted data from checkoutdto
        {

            //read list of order item objects
            //request contains the cookie, response that allows us to delete the cookie, context allows us to read product details from DB
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);//pass in the list obtained from GetCartItems
            //pass data to the view using viewbag
            ViewBag.CartItems = cartItems;
            ViewBag.ShippingFee = shippingFee; //from config
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal + shippingFee;

            if (!ModelState.IsValid)//if not valid return the view and the model
            { 
                return View(model);
            }
            //check if shopping cart is empty or not
            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessage = "Your Cart is empty";
                return View(model);
            }
            /*use temp data to send data from one action to another*/
            TempData["DeliveryAddress"] = model.DeliveryAddress;//submitted data from form
            TempData["PaymentMethod"] = model.PaymentMethod;
            /*if cart not empty refer user to confirmed page action of this controller
             it takes in delivery address and payment method from the model passed in*/
            return RedirectToAction("Confirm");
        }

        public IActionResult Confirm() //display confirm page
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = CartHelper.GetSubtotal(cartItems) + shippingFee;
            int cartSize = 0;
            foreach (var item in cartItems)//for every item in the list, update cart size.
            {
                cartSize += item.Quantity;
            }
            /*Can only read data from temp data one time so need to call keep to keep the temp
             data like this so we can read them several times*/
            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();
            //check if we have valid values and cartsize
            if (cartSize == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");//redirect user to home page
            }
            //otehrwise pass some data to the view using viewbag
            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Total = total;
            ViewBag.CartSize = cartSize;


            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Confirm(int any)//submit confirm page.method accessible using httppost and only authnticated users. using a method already created so need to have a parameter. we can use dummy data if not using parameters and int has a default value so we cna use it with no errors
        {
            //read order details. read order items from cart
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if (cartItems.Count == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            //read current user
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //create and save an order
            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                ShippingFee = shippingFee,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = "pending",
                PaymentDetails = "",
                OrderStatus = "created",
                CreatedAt = DateTime.Now,
            };

            context.Orders.Add(order);
            context.SaveChanges();

            //delete shopping cart cookie
            Response.Cookies.Delete("shopping_cart");

            ViewBag.SuccessMessage = "Order created successfully";

            return View();
        }

    }
}
