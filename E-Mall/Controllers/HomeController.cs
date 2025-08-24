using System.Diagnostics;
using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Mall.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, EDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            //Count
            int storescount = _context.Stores.Count();
            var role = await _roleManager.FindByNameAsync("Customer");
            int customerscount = _context.UserRoles.Count(ur => ur.RoleId == role.Id);
            int productscount = _context.Products.Count();

            ViewData["stores"] = storescount;
            ViewData["customer"] = customerscount;
            ViewData["products"] = productscount;

            var featuredStores = await _context.Stores
                .Where(s => s.Featured == true)
                .ToListAsync();

            var featuredProducts = await _context.Products
                .Where(p => p.Featured == true)
                .ToListAsync();

            var viewModel = new HomePageViewModel
            {
                FeaturedStores = featuredStores,
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult deliverydrivers()
        {
            return View();
        }
        public async Task<IActionResult> Shops()
        {
            var stores = await _context.Stores
                .Include(s => s.Category)
                .Include(s => s.Reviews) // Include the reviews collection
                .Where(s => s.IsActive)
                .ToListAsync();



            // For each store, calculate the average rating from the reviews
            foreach (var store in stores)
            {
                if (store.Reviews != null && store.Reviews.Any())
                {
                    // Calculate average from actual reviews
                    store.Rating = store.Reviews.Average(r => r.Rating);
                }
                else
                {
                    // No reviews yet
                    store.Rating = 0;
                }
            }

            return View(stores);
        }

        public async Task<IActionResult> products()
        {
            var allProducts = await _context.Products.ToListAsync();
            return View(allProducts);
        }
        public IActionResult about()
        {
            return View();
        }

        public IActionResult contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
