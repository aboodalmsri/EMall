using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
//using E_Mall.ViweModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Claims;
using System.Threading.Tasks;


namespace E_Mall.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly UserManager<E_MallUser> _userManager;
        private readonly EDbContext _context;
        private readonly ILogger<CustomerController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;


        public CustomerController(UserManager<E_MallUser> userManager, EDbContext context, ILogger<CustomerController> logger, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> cart()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning("User not found while accessing cart.");
                    return Unauthorized(); 
                }

                var cart = await GetOrCreateCart(user.Id);

                // Load the cart items and their associated products
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
                ViewBag.TotalAmount = totalAmount;
                ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);


                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing cart index.");
                return StatusCode(500, "An error occurred while processing your request."); // Or return an error view
            }
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public async Task<IActionResult> AddToCart(string productId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(productId) || quantity <= 0)
                {
                    _logger.LogWarning("Invalid product ID or quantity provided.");
                    return BadRequest("Invalid product ID or quantity.");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found while adding to cart.");
                    return Unauthorized();
                }

                var cart = await GetOrCreateCart(user.Id);

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {productId} not found.");
                    return NotFound("Product not found.");
                }

                // Check if the item is already in the cart
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (existingCartItem != null)
                {
                    // Update the quantity if the item already exists
                    existingCartItem.Quantity += quantity;
                    _context.Update(existingCartItem);
                    _logger.LogInformation($"Updated quantity for product {productId} in cart {cart.Id}. New quantity: {quantity}");
                }
                else
                {
                    // Create a new cart item
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
                    _logger.LogInformation($"Added product {productId} to cart {cart.Id} with quantity {quantity}.");
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding to cart.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(string cartItemId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(cartItemId) || quantity <= 0)
                {
                    _logger.LogWarning("Invalid cart item ID or quantity provided.");
                    return BadRequest("Invalid cart item ID or quantity.");
                }

                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item with ID {cartItemId} not found.");
                    return NotFound("Cart item not found.");
                }

                cartItem.Quantity = quantity;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated quantity of cart item {cartItemId} to {quantity}.");
                return RedirectToAction(nameof(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cart quantity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(string cartItemId)
        {
            try
            {
                if (string.IsNullOrEmpty(cartItemId))
                {
                    _logger.LogWarning("Invalid cart item ID provided.");
                    return BadRequest("Invalid cart item ID.");
                }

                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item with ID {cartItemId} not found.");
                    return NotFound("Cart item not found.");
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Removed cart item {cartItemId} from cart.");
                return RedirectToAction(nameof(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing from cart.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Helper method to get or create the user's cart
        private async Task<Cart> GetOrCreateCart(string userId)
        {
            try
            {
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { Id = Guid.NewGuid().ToString(), UserId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new cart for user {userId} with cart ID {cart.Id}.");
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting or creating cart for user {userId}.");
                throw; // Re-throw the exception so the calling method can handle it
            }
        }





        public async Task<IActionResult> Checkout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                var cart = await GetOrCreateCart(user.Id);

                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();
                if (cartItems.Count == 0)
                {
                    TempData["Message"] = "سلّة الشراء فارغة."; //Show a message if user tries to check out without items in the cart
                    return RedirectToAction("Index", "Cart"); //Redirect back to the cart
                }
                decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
                ViewBag.TotalAmount = totalAmount;
                ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);

                return View("Checkout", cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while navigating to the checkout page.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                var cart = await GetOrCreateCart(user.Id);

                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                if (cartItems.Count == 0)
                {
                    return BadRequest("The cart is empty.");
                }

                // Create a new order
                var order = new Order
                {
                    UserId = user.Id,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price),
                    Status = "قيد المعالجة" // Set the default status
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Clear the cart
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {order.Id} created for user {user.Id}.");

                return RedirectToAction("OrderConfirmation"); // Redirect to the confirmation page
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the order.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }



        public async Task<IActionResult> dashbord()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart


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
        public async Task<IActionResult> favorite()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            try
            {

                if (user == null)
                {
                    return Unauthorized();
                }

                // Get or Create wishlist for user
                var wishlist = await GetOrCreateWishlist(user.Id);

                // Load the wishlist items and their associated products
                var wishlistItems = await _context.WishlistItems
                    .Include(wi => wi.Product)
                    .Where(wi => wi.WishlistId == wishlist.Id)
                    .ToListAsync();

                //Pass the data to the view
                return View("Favorite", wishlistItems);  // Pass the WishlistItems to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing wishlist index.");
                return StatusCode(500, "An error occurred while processing your request.");
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return BadRequest("Invalid product ID.");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var wishlist = await GetOrCreateWishlist(user.Id);

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Check if the item is already in the wishlist
                var existingWishlistItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

                if (existingWishlistItem != null)
                {
                    // Item already exists - do nothing, or return a message
                    TempData["Message"] = "This item is already in your wishlist.";
                    return RedirectToAction("ProductDetails", "Customer", new { id = productId });
                }

                // Create a new wishlist item
                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId
                };
                _context.WishlistItems.Add(wishlistItem);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Item added to your wishlist!";

                _logger.LogInformation($"Added product {productId} to wishlist {wishlist.Id}.");

                return RedirectToAction("ProductDetails", "Customer", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding to wishlist.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // POST: Wishlist/RemoveFromWishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(string wishlistItemId)
        {
            try
            {
                if (string.IsNullOrEmpty(wishlistItemId))
                {
                    return BadRequest("Invalid wishlist item ID.");
                }

                var wishlistItem = await _context.WishlistItems.FindAsync(wishlistItemId);
                if (wishlistItem == null)
                {
                    return NotFound("Wishlist item not found.");
                }

                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Removed wishlist item {wishlistItemId} from wishlist.");

                return RedirectToAction("products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing from wishlist.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Helper method to get or create the user's wishlist
        private async Task<Wishlist> GetOrCreateWishlist(string userId)
        {
            try
            {
                var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId);

                if (wishlist == null)
                {
                    wishlist = new Wishlist { Id = Guid.NewGuid().ToString(), UserId = userId };
                    _context.Wishlists.Add(wishlist);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new wishlist for user {userId} with wishlist ID {wishlist.Id}.");
                }

                return wishlist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting or creating wishlist for user {userId}.");
                throw; // Re-throw the exception
            }
        }



        public async Task<IActionResult> orders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart


            try
            {

                if (user == null)
                {
                    return Unauthorized();
                }

                // Retrieve the user's orders from the database
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product) // Include products for each order item
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderDate) // Order by date, newest first
                    .ToListAsync();

                return View("Orders", orders); // Pass data to view named "Orders"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing orders index.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest("Invalid order ID.");
                }

                var order = await _context.Orders
                    .Include(o => o.OrderItems)  // Include order items for deletion
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                var user = await _userManager.GetUserAsync(User);

                if (order.UserId != user.Id)
                {
                    return Forbid(); // Or return UnauthorizedResult or RedirectToAction with an error message
                }

                if (order.Status != "قيد المعالجة")
                {
                    TempData["Message"] = "لا يمكن إلغاء هذا الطلب لأنه ليس في حالة \"قيد المعالجة\".";
                    return RedirectToAction("orders");
                }

                // Remove order items
                _context.OrderItems.RemoveRange(order.OrderItems);

                // Remove the order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} and its items deleted.");
                TempData["CMessage"] = "تم إلغاء الطلب وحذفه بنجاح.";

                return RedirectToAction("orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while cancelling and deleting order {orderId}.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        public async Task<IActionResult> products()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            var allproducts = await _context.Products
                .Include(s => s.ProductReviews)
                .ToListAsync();

            foreach (var product in allproducts)
            {
                if (product.ProductReviews != null && product.ProductReviews.Any())
                {
                    product.Rating = product.ProductReviews.Average(r => r.Rating);
                }
                else
                {
                    product.Rating = 0;
                }
            }

            return View(allproducts);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            var user1 = await _userManager.GetUserAsync(User);

            if (user1 == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                FirstName = user1.FirstName,
                LastName = user1.LastName,
                PhoneNumber = user1.PhoneNumber,
                Email = user1.Email // Optional: Include if you want to display the Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF attacks
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return to the form with validation errors
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Update the user's properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            // Save the changes
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Display a success message
                TempData["SuccessMessage"] = "تم تحديث الملف الشخصي بنجاح.";
                return RedirectToAction("Profile"); // Redirect to the profile page
            }
            else
            {
                // Display error messages
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model); // Return to the form with error messages
            }
        }
        public async Task<IActionResult> reviews()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }


            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart


            var storeReviews = await _context.Reviews
                .Where(r => r.CustomerId == user.Id)
                .Include(r => r.Store)
                .ToListAsync();

            var productReviews = await _context.ProductReviews
                .Where(r => r.CustomerId == user.Id)
                .Include(r => r.Product) 
                .ToListAsync();

            var viewModel = new ReviewsViewModel
            {
                StoreReviews = storeReviews,
                ProductReviews = productReviews
            };

            return View(viewModel);
        }
        public class ReviewsViewModel
        {
            public List<Review> StoreReviews { get; set; } = new List<Review>();
            public List<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        }



        public async Task<IActionResult> Shops()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

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

        [Route("/StoreDetails/{id}")]
        public async Task<IActionResult> StoreDetails(string id) // Changed to string
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            ViewBag.StoreProducts = await _context.Products
                .Where(p => p.StoreId == id)
                .ToListAsync();

            // Get reviews for this store
            var reviews = await _context.Reviews
                .Where(r => r.StoreId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;

            // Calculate average rating
            if (reviews.Any())
            {
                // In your controller
                ViewBag.AverageRating = reviews.Any() ? (double)reviews.Average(r => r.Rating) : 0.0;
                ViewBag.ReviewCount = reviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            return View(store);
        }

        [HttpPost]
        [Authorize] // Ensure user is logged in
        public async Task<IActionResult> AddReview(string storeId, int rating, string comment) 
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("Rating", "التقييم يجب أن يكون بين 1 و 5 نجوم");
                return RedirectToAction("StoreDetails", new { id = storeId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.StoreId == storeId && r.CustomerId == userId);

            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.Now;
            }
            else
            {
                // Create new review
                var review = new Review
                {
                    Id = Guid.NewGuid().ToString(),
                    StoreId = storeId,
                    CustomerId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("StoreDetails", new { id = storeId });
        }

        public async Task<IActionResult> ProductDetails(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart
            var product = _context.Products.Include(p => p.ProductReviews).Include(p => p.Store).FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;

            // Calculate average rating
            if (reviews.Any())
            {
                // In your controller
                ViewBag.AverageRating = reviews.Any() ? (double)reviews.Average(r => r.Rating) : 0.0;
                ViewBag.ReviewCount = reviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            return View(product);
        }


        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> AddReviewP(string ProductId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("Rating", "التقييم يجب أن يكون بين 1 و 5 نجوم");
                return RedirectToAction("productdetails", new { id = ProductId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == ProductId && r.CustomerId == userId);

            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.Now;
            }
            else
            {
                // delete productid1
                var review = new ProductReview
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = ProductId,
                    CustomerId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.ProductReviews.Add(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("productdetails", new { id = ProductId });
        }

        public async Task<IActionResult> deliverydrivers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart
            return View();
        }

        public async Task<IActionResult> About()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart


            return View();
        }

        public async Task<IActionResult> Myfeedbacks()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return NotFound("المستخدم غير موجود.");
            //}

            // جلب البلاغات الخاصة بالمستخدم المسجل دخوله
            var myContacts = await _context.Contacts
                .Where(f => f.Email == user.Email)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // اختبار وجود بيانات
            if (!myContacts.Any())
            {
                ViewBag.Message = "لا توجد بلاغات مسجلة لديك.";
            }

            return View(myContacts);
        }
        public IActionResult contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Contacts.Add(contact);
                    _context.SaveChanges();
                    ViewBag.SuccessMessage = "Thank you for your feedback!";
                    ModelState.Clear();
                    return View(new Contact());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    ViewBag.Errormsg = "Something went wrong!";
                    return View(contact);
                }
            }
            return View(contact);
        }
        public async Task<IActionResult> Privacy()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User not found while accessing cart.");
                return Unauthorized();
            }

            var cart = await GetOrCreateCart(user.Id);

            // Load the cart items and their associated products
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItemCount = cartItems.Sum(item => item.Quantity);
            //cart

            return View();
        }
    }
}
