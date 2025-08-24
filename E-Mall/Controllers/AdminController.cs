using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using E_Mall.Models.ViewModels;

namespace E_Mall.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<E_MallUser> _userManager;
        private readonly EDbContext _context;
        private readonly ILogger<TraderController> _logger;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration _configuration;


        public AdminController(UserManager<E_MallUser> userManager, EDbContext context, ILogger<TraderController> logger, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            this.environment = environment;
            _configuration = configuration;
        }

        public async Task<IActionResult> contact()
        {

            var contacts = await _context.Contacts
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(contacts);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deletecontact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting feedback: " + ex.Message;
            }

            return RedirectToAction(nameof(contact));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyTocontact(int id, string Subject, string Message, string Email)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                // حفظ الرد داخل قاعدة البيانات
                contact.IsReplied = true;
                contact.RepliedAt = DateTime.Now;
                contact.ReplyMessage = Message; // ✅ **جديد: حفظ نص الرد**  

                await _context.SaveChangesAsync(); // حفظ التغييرات في قاعدة البيانات

                TempData["SuccessMessage"] = "تم إرسال الرد وحفظه في قاعدة البيانات بنجاح!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إرسال الرد: " + ex.Message;
            }

            return RedirectToAction(nameof(contact));
        }

        // GET: Admin/ProductManagement
        public IActionResult Feedbacks()
        {
            return View();
        }
        public async Task<IActionResult> ProductManagement()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Admin/CreateP
        public IActionResult CreateP()
        {
            return View();
        }

        // POST: Admin/CreateP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateP(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductManagement));
            }
            return View(product);
        }

        // GET: Admin/EditP/5
        public async Task<IActionResult> EditP(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Admin/EditP/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditP(string id, AddProductsModel addProductsModel)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update the properties of the existing product
                    existingProduct.Name = addProductsModel.Name;
                    existingProduct.Description = addProductsModel.Description;
                    existingProduct.Price = addProductsModel.Price;
                    existingProduct.Category = addProductsModel.Category;
                    existingProduct.Featured = addProductsModel.Featured;
                    existingProduct.QuantityInStock = addProductsModel.QuantityInStock;
                    existingProduct.HaveDiscount = addProductsModel.HaveDiscount;
                    existingProduct.IsNew = addProductsModel.IsNew;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    ModelState.AddModelError("", "حدث خطأ أثناء تعديل المنتج.");
                }
                return RedirectToAction("ProductManagement", "Admin");
            }
            return View(addProductsModel);
        }

        // GET: Admin/DeleteP/5
        public async Task<IActionResult> DeleteP(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteP")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedP(string id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductManagement));
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        public IActionResult dashbord()
        {
            ViewBag.TotalSales = _context.Orders.Sum(o => o.TotalAmount);
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.StoreCount = _context.Stores.Count();

            ViewBag.LatestUsers = _context.Users
                .OrderByDescending(u => u.Id)
                .Take(3)
                .ToList();

            ViewBag.LatestOrders = _context.Orders
                .Include(o => o.User) 
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .ToList();

            var salesData = new List<decimal>();
            var monthLabels = new List<string>();

            for (int i = 11; i >= 0; i--) 
            {
                DateTime monthStart = DateTime.Now.AddMonths(-i).Date.AddDays(1 - DateTime.Now.AddMonths(-i).Date.Day);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                //Calculate total sales for each month
                decimal monthlySales = _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate <= monthEnd)
                    .Sum(o => o.TotalAmount);

                salesData.Add(monthlySales);
                monthLabels.Add(monthStart.ToString("MMM")); // Get the month name (e.g., "Jan")
            }

            ViewBag.SalesData = salesData;
            ViewBag.MonthLabels = monthLabels;


            return View(); // Don't need to pass a model to the view
        }

        public IActionResult usermanagement()
        {
            return View();
        }
        public async Task<IActionResult> MerchantManagement(string searchString)
        {
            var merchants = from m in _context.Merchants select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                merchants = merchants.Where(s => s.MerchantName.Contains(searchString)
                                       || s.MerchantEmail.Contains(searchString)
                                       || s.MerchantPhone.Contains(searchString));
            }

            return View(await merchants.ToListAsync());
        }
        // GET: Admin/CreateM
        public IActionResult CreateM()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateM(string MerchantEmail, string MerchantName, string MerchantPhone, string StoreName, string StoreLink, bool IsActive, string Password, string FirstName, string LastName)
        {
            // إنشاء مستخدم جديد في AspNetUsers
            var user = new E_MallUser
            {
                UserName = MerchantEmail,
                Email = MerchantEmail,
                EmailConfirmed = true,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = MerchantPhone
            };
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // إضافة المستخدم إلى دور "Trader"
                var roleResult = await _userManager.AddToRoleAsync(user, "Trader");

                if (!roleResult.Succeeded)
                {
                    // إذا فشلت إضافة الدور، قم بمعالجة الأخطاء
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // يمكنك أيضًا حذف المستخدم الذي تم إنشاؤه للتو إذا فشلت إضافة الدور
                    await _userManager.DeleteAsync(user);
                    return View(); // العودة إلى العرض مع عرض الأخطاء
                }

                // إنشاء تاجر جديد
                var merchant = new Merchant
                {
                    MerchantName = FirstName + " " + LastName,
                    MerchantEmail = MerchantEmail,
                    MerchantPhone = MerchantPhone,
                    StoreName = StoreName,
                    StoreLink = StoreLink,
                    IsActive = IsActive,
                    UserId = user.Id // Set the UserId to the newly created user's ID
                };

                _context.Merchants.Add(merchant);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MerchantManagement));
            }
            else
            {
                // في حالة فشل إنشاء المستخدم، قم بعرض الأخطاء في النموذج
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
        }

        // GET: Admin/EditM/5
        public async Task<IActionResult> EditM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchant = await _context.Merchants
                .Include(m => m.User) // Load the related User object
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
            {
                return NotFound();
            }

            return View(merchant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditM(int id, string FirstName, string LastName, string MerchantEmail, string MerchantPhone, string StoreName, string StoreLink, bool IsActive)
        {
            if (ModelState.IsValid)
            {
                var merchant = await _context.Merchants
                    .Include(m => m.User) // Load the related User object
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (merchant == null)
                {
                    return NotFound();
                }

                try
                {
                    // Get the existing user
                    var user = merchant.User;

                    if (user == null)
                    {
                        return NotFound("User not found for this merchant.");
                    }

                    // Update AspNetUsers
                    user.UserName = MerchantEmail;
                    user.Email = MerchantEmail;
                    user.FirstName = FirstName;
                    user.LastName = LastName;
                    user.PhoneNumber = MerchantPhone;

                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        // Log or display errors from user deletion
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(merchant);
                    }

                    merchant.MerchantName = FirstName + " " + LastName;
                    merchant.MerchantEmail = MerchantEmail;
                    merchant.MerchantPhone = MerchantPhone;
                    merchant.StoreName = StoreName;
                    merchant.StoreLink = StoreLink;
                    merchant.IsActive = IsActive;

                    _context.Update(merchant);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(MerchantManagement));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MerchantExists(merchant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View();
        }

        // GET: Admin/DeleteM/5
        public async Task<IActionResult> DeleteM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
            {
                return NotFound();
            }

            return View(merchant);
        }

        // POST: Admin/DeleteM/5
        [HttpPost, ActionName("DeleteM")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedM(int id)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant != null)
            {
                // Get the user to delete
                var user = merchant.User;
                if (user != null)
                {
                    try
                    {
                        var result = await _userManager.DeleteAsync(user);

                        if (!result.Succeeded)
                        {
                            // Log or display errors from user deletion
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(merchant);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // Log the concurrency exception
                        _logger.LogError(ex, "Concurrency exception occurred while deleting user.");

                        // Handle the concurrency issue:
                        ModelState.AddModelError(string.Empty, "The record was modified or deleted by another user. Please refresh and try again.");
                        return View(merchant); // Return to the delete confirmation view with an error message
                    }
                }
                // Delete DeliveryDriver (if you don't use cascade delete)
                //_context.DeliveryDrivers.Remove(deliveryDriver);
                //await _context.SaveChangesAsync();  //Remove this line due to cascading delete and already deleted

            }
            else
            {
                // If the DeliveryDriver is already deleted, redirect to the list with a message
                TempData["ErrorMessage"] = "The Merchant record was not found.";
                return RedirectToAction(nameof(MerchantManagement));
            }

            return RedirectToAction(nameof(MerchantManagement));
        }

        private bool MerchantExists(int id)
        {
            return _context.Merchants.Any(e => e.Id == id);
        }

        public async Task<IActionResult> StoreManagement()
        {
            var stores = _context.Stores.Include(s => s.Merchant).Include(s => s.Category);
            return View(await stores.ToListAsync());
        }
        public IActionResult CreateS()
        {
            ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName");
            ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // حماية من هجمات تزوير الطلبات عبر المواقع
        public async Task<IActionResult> CreateS(AddStoreModel addStoreModel)
        {
            if (addStoreModel.Logo == null)
            {
                ModelState.AddModelError("Logo", "صورة الشعار مطلوبة");
            }
            if (addStoreModel.CoverImage == null)
            {
                ModelState.AddModelError("CoverImage", "صورة الغلاف مطلوبة");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName", addStoreModel.MerchantId);
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", addStoreModel.CategoryId);
                return View(addStoreModel);
            }

            try
            {
                // حفظ صور الشعار والغلاف
                string logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(addStoreModel.Logo.FileName);
                string coverImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(addStoreModel.CoverImage.FileName);

                string logoPath = Path.Combine(environment.WebRootPath, "logos", logoFileName);
                string coverImagePath = Path.Combine(environment.WebRootPath, "covers", coverImageFileName);

                using (var logoStream = new FileStream(logoPath, FileMode.Create))
                {
                    await addStoreModel.Logo.CopyToAsync(logoStream);
                }

                using (var coverImageStream = new FileStream(coverImagePath, FileMode.Create))
                {
                    await addStoreModel.CoverImage.CopyToAsync(coverImageStream);
                }

                // إنشاء كائن Store
                var store = new Store
                {
                    Id = Guid.NewGuid().ToString(), // إنشاء GUID جديد كـ Id
                    Name = addStoreModel.Name,
                    Logo = logoFileName,
                    CoverImage = coverImageFileName,
                    Description = addStoreModel.Description,
                    Featured = addStoreModel.Featured,
                    Location = addStoreModel.Location,
                    ProductCount = addStoreModel.ProductCount,
                    MerchantId = addStoreModel.MerchantId,
                    CategoryId = addStoreModel.CategoryId,
                    IsActive = addStoreModel.IsActive
                };

                _context.Stores.Add(store);

                //add +1 to storecount for category
                var category = await _context.Categories.FindAsync(addStoreModel.CategoryId);
                if(category != null)
                {
                    category.StoreCount++;
                    _context.Categories.Update(category);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction("StoreManagement", "Admin");
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء المتجر.");
                ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName", addStoreModel.MerchantId);
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", addStoreModel.CategoryId);
                return View(addStoreModel);
            }
        }

        public async Task<IActionResult> EditS(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores
                .AsNoTracking() 
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
            {
                return NotFound();
            }

            ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName", store.MerchantId);
            ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", store.CategoryId);

            var addStoreModel = new AddStoreModel
            {
                Name = store.Name,
                Description = store.Description,
                Featured = store.Featured,
                Location = store.Location,
                ProductCount = store.ProductCount,
                MerchantId = store.MerchantId,
                CategoryId = store.CategoryId,
                IsActive = store.IsActive
            };

            ViewData["StoreId"] = store.Id;
            ViewData["Logo"] = store.Logo;
            ViewData["Cover"] = store.CoverImage;

            return View(addStoreModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // حماية من هجمات تزوير الطلبات عبر المواقع
        public async Task<IActionResult> EditS(string id, AddStoreModel addStoreModel)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName", addStoreModel.MerchantId);
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", addStoreModel.CategoryId);
                ViewData["StoreId"] = store.Id;
                ViewData["Logo"] = store.Logo;
                ViewData["Cover"] = store.CoverImage;
                return View(addStoreModel);
            }

            try
            {
                var originalCategoryId = store.CategoryId;

                // تحديث صور الشعار والغطاء
                if (addStoreModel.Logo != null)
                {
                    // حذف الصورة القديمة
                    string oldLogoPath = Path.Combine(environment.WebRootPath, "logos", store.Logo);
                    if (System.IO.File.Exists(oldLogoPath))
                    {
                        System.IO.File.Delete(oldLogoPath);
                    }

                    // حفظ الصورة الجديدة
                    string logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(addStoreModel.Logo.FileName);
                    string logoPath = Path.Combine(environment.WebRootPath, "logos", logoFileName);
                    using (var stream = new FileStream(logoPath, FileMode.Create))
                    {
                        await addStoreModel.Logo.CopyToAsync(stream);
                        store.Logo = logoFileName;
                    }
                }

                if (addStoreModel.CoverImage != null)
                {
                    // حذف الصورة القديمة
                    string oldCoverImagePath = Path.Combine(environment.WebRootPath, "covers", store.CoverImage);
                    if (System.IO.File.Exists(oldCoverImagePath))
                    {
                        System.IO.File.Delete(oldCoverImagePath);
                    }

                    // حفظ الصورة الجديدة
                    string coverImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(addStoreModel.CoverImage.FileName);
                    string coverImagePath = Path.Combine(environment.WebRootPath, "covers", coverImageFileName);
                    using (var stream = new FileStream(coverImagePath, FileMode.Create))
                    {
                        await addStoreModel.CoverImage.CopyToAsync(stream);
                        store.CoverImage = coverImageFileName;
                    }
                }

                // تحديث خصائص المتجر
                store.Name = addStoreModel.Name;
                store.Description = addStoreModel.Description;
                store.Featured = addStoreModel.Featured;
                store.Location = addStoreModel.Location;
                store.ProductCount = addStoreModel.ProductCount;
                store.MerchantId = addStoreModel.MerchantId;
                store.CategoryId = addStoreModel.CategoryId;
                store.IsActive = addStoreModel.IsActive;

                _context.Stores.Update(store);

                // update category
                if (originalCategoryId != addStoreModel.CategoryId)
                {
                    var oldCategory = await _context.Categories.FindAsync(originalCategoryId);
                    if (oldCategory != null)
                    {
                        _context.Entry(oldCategory).State = EntityState.Modified;

                        if (oldCategory.StoreCount > 0)
                            oldCategory.StoreCount--;
                    }

                    var newCategory = await _context.Categories.FindAsync(addStoreModel.CategoryId);
                    if (newCategory != null)
                    {
                        _context.Entry(newCategory).State = EntityState.Modified;

                        newCategory.StoreCount++;

                    }
                }
                await _context.SaveChangesAsync(); // استخدام async/await

                return RedirectToAction("StoreManagement", "Admin");
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل المتجر.");
                ViewBag.MerchantId = new SelectList(_context.Merchants.ToList(), "Id", "MerchantName", addStoreModel.MerchantId);
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", addStoreModel.CategoryId);
                ViewData["StoreId"] = store.Id;
                ViewData["Logo"] = store.Logo;
                ViewData["Cover"] = store.CoverImage;
                return View(addStoreModel);
            }
        }

        public async Task<IActionResult> DeleteSAsync(string id)
        {
            var store = _context.Stores.Find(id);
            if(store == null)
            {
                return RedirectToAction("StoreManagement", "Admin");
            }

            string logofullpath = environment.WebRootPath + "/logos/" + store.Logo;
            System.IO.File.Delete(logofullpath);

            string coverfullpath = environment.WebRootPath + "/covers/" + store.CoverImage;
            System.IO.File.Delete(coverfullpath);

            _context.Stores.Remove(store);

            var category = await _context.Categories.FindAsync(store.CategoryId);
            if (category != null)
            {
                _context.Entry(category).State = EntityState.Modified;

                if (category.StoreCount > 0)
                    category.StoreCount--;
            }

            _context.SaveChanges(true);

            
            return RedirectToAction("StoreManagement", "Admin");

        }


        public async Task<IActionResult> ordermanagement()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("User Not Found!");
                return View();
            }
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ToListAsync();

                _logger.LogInformation($"Fetched {orders.Count} orders for the order management page.");

                return View("ordermanagement", orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders in OrderManagement action.");

                TempData["ErrorMessage"] = "An error occurred while loading the orders.";

                return View("ordermanagement", new System.Collections.Generic.List<Order>());
            }
        }

        public async Task<IActionResult> DeliveryDriverManagement(string searchString)
        {
            var deliveryDrivers = _context.DeliveryDrivers.AsNoTracking(); // يجلب البيانات دون تتبع

            if (!string.IsNullOrEmpty(searchString))
            {
                deliveryDrivers = deliveryDrivers.Where(s =>
                    s.DriverName.Contains(searchString) ||
                    s.DriverEmail.Contains(searchString) ||
                    s.DriverPhone.Contains(searchString) ||
                    s.CoverageArea.Contains(searchString)
                );
            }

            return View(await deliveryDrivers.ToListAsync());
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string DriverEmail, string DriverName, string DriverPhone, string DriverCarInfo, string CoverageArea, bool IsActive, string Password, string FirstName, string LastName)
        {
            // إنشاء مستخدم جديد في AspNetUsers
            var user = new E_MallUser
            {
                UserName = DriverEmail,
                Email = DriverEmail,
                EmailConfirmed = true,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = DriverPhone
            };
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // إضافة المستخدم إلى دور "DeliveryDriver"
                var roleResult = await _userManager.AddToRoleAsync(user, "Delivery");

                if (!roleResult.Succeeded)
                {
                    // إذا فشلت إضافة الدور، قم بمعالجة الأخطاء
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // يمكنك أيضًا حذف المستخدم الذي تم إنشاؤه للتو إذا فشلت إضافة الدور
                    await _userManager.DeleteAsync(user);
                    return View(); // العودة إلى العرض مع عرض الأخطاء
                }

                // إنشاء سائق التوصيل
                var deliveryDriver = new DeliveryDriver
                {
                    DriverName = FirstName + " " + LastName,
                    DriverEmail = DriverEmail,
                    DriverPhone = DriverPhone,
                    DriverCarInfo = DriverCarInfo,
                    CoverageArea = CoverageArea,
                    IsActive = IsActive,
                    UserId = user.Id // Set the UserId to the newly created user's ID
                };

                _context.DeliveryDrivers.Add(deliveryDriver);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(DeliveryDriverManagement));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
        }
        // GET: Admin/Edit
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var deliveryDriver = await _context.DeliveryDrivers
                .Include(m => m.User) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deliveryDriver == null)
            {
                return NotFound();
            }

            return View(deliveryDriver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string FirstName, string LastName, string DriverEmail, string DriverPhone, string DriverCarInfo, string CoverageArea, bool IsActive)
        {
            if (ModelState.IsValid)
            {
                var deliveryDriver = await _context.DeliveryDrivers
                    .Include(d => d.User) 
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (deliveryDriver == null)
                {
                    return NotFound();
                }

                try
                {
                    // Get the existing user
                    var user = deliveryDriver.User;

                    if (user == null)
                    {
                        return NotFound("User not found for this delivery driver.");
                    }

                    // Update AspNetUsers
                    user.UserName = DriverEmail;
                    user.Email = DriverEmail;
                    user.FirstName = FirstName;
                    user.LastName = LastName;
                    user.PhoneNumber = DriverPhone;

                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        // Log or display errors from user deletion
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(deliveryDriver);
                    }

                    deliveryDriver.DriverName = FirstName + " " + LastName;
                    deliveryDriver.DriverEmail = DriverEmail;
                    deliveryDriver.DriverPhone = DriverPhone;
                    deliveryDriver.DriverCarInfo = DriverCarInfo;
                    deliveryDriver.CoverageArea = CoverageArea;
                    deliveryDriver.IsActive = IsActive;

                    _context.Update(deliveryDriver);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(DeliveryDriverManagement));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeliveryDriverExists(deliveryDriver.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View();
        }
        // GET: Admin/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryDriver = await _context.DeliveryDrivers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deliveryDriver == null)
            {
                return NotFound();
            }

            return View(deliveryDriver);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deliveryDriver = await _context.DeliveryDrivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deliveryDriver != null)
            {
                // Get the user to delete
                var user = deliveryDriver.User;
                if (user != null)
                {
                    try
                    {
                        var result = await _userManager.DeleteAsync(user);

                        if (!result.Succeeded)
                        {
                            // Log or display errors from user deletion
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(deliveryDriver);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogError(ex, "Concurrency exception occurred while deleting user.");

                        ModelState.AddModelError(string.Empty, "The record was modified or deleted by another user. Please refresh and try again.");
                        return View(deliveryDriver); 
                    }
                }

            }
            else
            {
                TempData["ErrorMessage"] = "The delivery driver record was not found.";
                return RedirectToAction(nameof(DeliveryDriverManagement));
            }

            return RedirectToAction(nameof(DeliveryDriverManagement));
        }

        private bool DeliveryDriverExists(int id)
        {
            return _context.DeliveryDrivers.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email // Optional: Include if you want to display the Email
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
                TempData["SuccessMessage"] = "تم تحديث الملف الشخصي بنجاح.";
                return RedirectToAction("Profile"); 
            }
            else
            {
                // Display error messages
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model); 
            }
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        public IActionResult CreateC()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateC(AddCategoryModel addCategoryModel)
        {
            if (addCategoryModel.Image == null)
            {
                ModelState.AddModelError("Image", "صورة الفئة مطلوبة"); // Error message for missing image
                return View(addCategoryModel); // Return view if image is missing
            }

            if (!ModelState.IsValid)
            {
                return View(addCategoryModel);
            }

            string categoryimage = Guid.NewGuid().ToString() + Path.GetExtension(addCategoryModel.Image.FileName); // Get file extension from the filename
            string categoryimagePath = Path.Combine(environment.WebRootPath, "category", categoryimage);

            using (var cateStream = new FileStream(categoryimagePath, FileMode.Create))
            {
                await addCategoryModel.Image.CopyToAsync(cateStream); // Use CopyToAsync to save the file
            }

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = addCategoryModel.Name,
                Image = categoryimage
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");

        }
        public async Task<IActionResult> EditC(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewData["image"] = category.Image;

            return View(new AddCategoryModel
            {
                Name = category.Name
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditC(string id, AddCategoryModel model, IFormFile? Image)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (ModelState.ContainsKey("Image"))
            {
                ModelState["Image"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = model.Name;

                    if (Image != null && Image.Length > 0)
                    {
                        string fileName = Path.GetFileName(Image.FileName);
                        string filePath = Path.Combine("wwwroot/category", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }

                        category.Image = fileName;  
                    }

                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Categories));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(c => c.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["image"] = category.Image; 
            return View(model);
        }




        public async Task<IActionResult> DeleteCAsync(string id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return RedirectToAction("Categories", "Admin");
            }

            string logofullpath = environment.WebRootPath + "/category/" + category.Image;
            System.IO.File.Delete(logofullpath);


            _context.Categories.Remove(category);

            _context.SaveChanges(true);

            return RedirectToAction("Categories", "Admin");

        }
        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }







        public IActionResult Reports()
        {
            var viewModel = new ReportsViewModel();

            viewModel.ProductPerformances = _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ProductPerformance
                {
                    ProductName = _context.Products.FirstOrDefault(p => p.Id == g.Key).Name, // Get the product name
                    SalesCount = g.Sum(oi => oi.Quantity),
                    AverageRating = _context.Products.FirstOrDefault(p => p.Id == g.Key).Rating //Fetch Rating From Product Model
                })
                .OrderByDescending(p => p.SalesCount)
                .ToList();

            viewModel.TopStores = _context.Stores
                .Select(s => new TopStore
                {
                    StoreName = s.Name,
                    TotalRevenue = (decimal)_context.Products
                                      .Where(p => p.StoreId == s.Id)
                                      .Join(_context.OrderItems,
                                            p => p.Id,
                                            oi => oi.ProductId,
                                            (p, oi) => new { p, oi })
                                      .Sum(x => x.oi.Price * x.oi.Quantity)
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            return View(viewModel);
        }


        public IActionResult UserReviewsManagement(string userTypeFilter, int? ratingFilter, string searchName)
        {
            // Start with the base query
            IQueryable<Review> reviews = _context.Reviews.Include(r => r.Customer).Include(r => r.Store);

            // Apply filters
            if (!string.IsNullOrEmpty(userTypeFilter))
            {
                // This will depend on how you categorize users in relation to reviews.
                // Assuming you want to filter reviews based on the Customer's role.
                // You might need to adjust this logic based on your specific needs.
                reviews = reviews.Where(r =>
                    (userTypeFilter == "customer" && r.Customer != null) ||
                    (userTypeFilter == "store" && r.Store != null) ||
                    (userTypeFilter == "delivery" /*&& SomeConditionForDeliveryPerson*/)); // Adjust the condition
            }

            if (ratingFilter.HasValue)
            {
                reviews = reviews.Where(r => r.Rating == ratingFilter.Value);
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                reviews = reviews.Where(r =>
                    (r.Customer != null && (r.Customer.FirstName.Contains(searchName) || r.Customer.LastName.Contains(searchName))) ||
                    (r.Store != null && r.Store.Name.Contains(searchName)));
            }

            // Execute the query and convert to a list
            var reviewList = reviews.ToList();

            // Pass the reviews to the view
            return View(reviewList);
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
