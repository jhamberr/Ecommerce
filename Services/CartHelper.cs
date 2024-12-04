using Ecommerce.Models;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace Ecommerce.Services
{
    public class CartHelper
    {
        /*this helper service will help us to read the shopping cart on the server and set the cart size
         * on the server.how to read the shopping cart cookie on the server and display the cart size
         * on the server. this function helps with reading the shopping cart cookie and helping to
         * set and display the cart size.  this firth method allows us to read the shopping cart
         * cookie. request contains shopping cart and response allows us to delete cart if necessary
         product ID and order quantity dictionary pairs. this method allows us to read the shopping cart cookie as
        dictionary
        */
        public static Dictionary<int,int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {
            //read shopping cart cookie from request named "shopping_cart"
            string cookieValue = request.Cookies["shopping_cart"] ?? "";
            //convert string to dictionary. add try catch block to handle any exceptions with conversion
            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));//decode from base64 to string json object
                Console.WriteLine("[CartHelper] cart=" + cookieValue + " -> " + cart);//display value of encoded value and decoded value of shopping cart
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);//convert/deserialize json object to dictionary pair of integer integer
                if (dictionary != null)
                {
                    return dictionary;
                }
            }
            catch (Exception)//empty or bad value of cookie
            {

            }
            if (cookieValue.Length > 0)//case for bad value
            {
                // this cookie is not valid => delete it
                response.Cookies.Delete("shopping_cart");
            }
            return new Dictionary<int, int>();//return empty dictionary if dictionary is null from above
        }

        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            int cartSize = 0;

            var CartDictionary = GetCartDictionary(request, response);//list of cookies
            foreach (var keyValuePair in CartDictionary)
            {
                cartSize += keyValuePair.Value;
            }

            return cartSize;
        }

        public static List<OrderItem> GetCartItems(HttpRequest request, HttpResponse response
            , ApplicationDbContext context)
        {
            var cartItems = new List<OrderItem>();

            var cartDictionary = GetCartDictionary(request, response);
            foreach (var pair in cartDictionary)
            {
                int productId = pair.Key;
                int quantity = pair.Value;
                var product = context.Products.Find(productId);
                if (product == null) continue;
                //create order item
                var item = new OrderItem
                {
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Product = product,
                };
                //add to list of items
                cartItems.Add(item);
            }
            return cartItems;
        }
        //get subtotal of shopping cart

        public static decimal GetSubtotal(List<OrderItem> cartItems)//get order item objects that we retrieved from GetCartItems
        {
            decimal subtotal = 0;
            
            foreach (var item in cartItems)
            {
                subtotal += item.Quantity * item.UnitPrice;//update subtotal for every item in the list
            }

            return subtotal;
        }
    }
}
