using Ecommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //context represents  a DB session
        public ApplicationDbContext(DbContextOptions options) : base (options)
        {

        }

       public DbSet<Product> Products { get; set; }//products is name of table in DB
        public DbSet<Order> Orders { get; set; }/*dont need order items set because we can access items 
                                                 * from orders property so make orders table. order item table
                                                 * will be recursively made*/
    }
}
