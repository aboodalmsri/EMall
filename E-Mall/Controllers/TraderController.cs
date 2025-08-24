using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
using E_Mall.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace E_Mall.Controllers
{
    [Authorize(Roles = "Trader")]
    public class TraderController : Controller
    {
        private readonly UserManager<E_MallUser> _userManager;
        private readonly EDbContext _context;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<TraderController> _logger;


        public TraderController(UserManager<E_MallUser> userManager, EDbContext context, IWebHostEnvironment environment, ILogger<TraderController> logger)
        {
            _userManager = userManager;
            _context = context;
            this.environment = environment;
            _logger = logger;
        }
        public IActionResult dashbord()
        {
            return View();
        }
        public IActionResult customermanagement()
        {
            return View();
        }
        public async Task<IActionResult> offermanagement()
        {
            var activeOffers = await _context.Offers
                .Where(o => o.EndDate >= DateTime.Now)
                .ToListAsync();

            var expiredOffers = await _context.Offers
                .Where(o => o.EndDate < DateTime.Now)
                .ToListAsync();

            ViewData["ActiveOffers"] = activeOffers;
            ViewData["ExpiredOffers"] = expiredOffers;

            return View();
        }
        [HttpGet]
        public IActionResult CreateO()
        {
            return View();
        }

        // إنشاء عرض جديد - معالجة البيانات المرسلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateO(OfferViewModel model)
        {
            if (ModelState.IsValid)
            {
                // إنشاء كود كوبون عشوائي إذا كان نوع العرض كوبون ترويجي
                string couponCode = null;
                if (model.Type == "كوبون ترويجي")
                {
                    couponCode = GenerateRandomCouponCode();
                }

                var offer = new Offer
                {
                    Name = model.Name,
                    Type = model.Type,
                    Value = model.Value,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CouponCode = couponCode,
                    UsageCount = 0,
                    TotalDiscountAmount = 0
                };

                _context.Add(offer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(offermanagement));
            }
            return View(model);
        }

        // تفاصيل العرض
        public async Task<IActionResult> DetailsO(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // تحرير العرض - عرض نموذج التحرير
        [HttpGet]
        public async Task<IActionResult> EditO(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            var viewModel = new OfferViewModel
            {
                Id = offer.Id,
                Name = offer.Name,
                Type = offer.Type,
                Value = offer.Value,
                StartDate = offer.StartDate,
                EndDate = offer.EndDate
            };

            return View(viewModel);
        }

        // تحرير العرض - معالجة البيانات المرسلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditO(int id, OfferViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var offer = await _context.Offers.FindAsync(id);
                    if (offer == null)
                    {
                        return NotFound();
                    }

                    offer.Name = model.Name;
                    offer.Type = model.Type;
                    offer.Value = model.Value;
                    offer.StartDate = model.StartDate;
                    offer.EndDate = model.EndDate;

                    // إعادة توليد كود الكوبون إذا تم تغيير النوع إلى كوبون ترويجي
                    if (model.Type == "كوبون ترويجي" && offer.Type != "كوبون ترويجي")
                    {
                        offer.CouponCode = GenerateRandomCouponCode();
                    }
                    else if (model.Type != "كوبون ترويجي")
                    {
                        offer.CouponCode = null;
                    }

                    _context.Update(offer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfferExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(offermanagement));
            }
            return View(model);
        }

        // حذف العرض - عرض صفحة تأكيد الحذف
        [HttpGet]
        public async Task<IActionResult> DeleteO(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // حذف العرض - تأكيد الحذف
        [HttpPost, ActionName("DeleteO")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offer = await _context.Offers.FindAsync(id);
            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(offermanagement));
        }

        // التحقق من وجود العرض
        private bool OfferExists(int id)
        {
            return _context.Offers.Any(e => e.Id == id);
        }

        // دالة لتوليد كود كوبون عشوائي
        private string GenerateRandomCouponCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // إنشاء كود من 8 أحرف
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // التحقق من صلاحية الكوبون
        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { valid = false, message = "الرجاء إدخال كود الكوبون" });
            }

            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.CouponCode == code && o.Type == "كوبون ترويجي");

            if (offer == null)
            {
                return Json(new { valid = false, message = "كود الكوبون غير صحيح" });
            }

            if (DateTime.Now < offer.StartDate)
            {
                return Json(new { valid = false, message = "هذا الكوبون غير نشط بعد" });
            }

            if (DateTime.Now > offer.EndDate)
            {
                return Json(new { valid = false, message = "انتهت صلاحية هذا الكوبون" });
            }

            return Json(new
            {
                valid = true,
                message = "الكوبون صالح",
                offerName = offer.Name,
                discountValue = offer.Value,
                offerType = offer.Type
            });
        }

        // استخدام الكوبون وزيادة عداد الاستخدام
        [HttpPost]
        public async Task<IActionResult> UseCoupon(string code, decimal amount)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("كود الكوبون مطلوب");
            }

            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.CouponCode == code && o.Type == "كوبون ترويجي");

            if (offer == null || DateTime.Now < offer.StartDate || DateTime.Now > offer.EndDate)
            {
                return BadRequest("الكوبون غير صالح");
            }

            offer.UsageCount++;
            offer.TotalDiscountAmount += amount;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم استخدام الكوبون بنجاح" });
        }
        public IActionResult ordermanagement()
        {
            return View();
        }
        // GET: Trader/ProductManagement
        public async Task<IActionResult> ProductManagement(string searchString)
        {
            // Get the current merchant's ID
            int currentMerchantId = GetCurrentMerchantId();

            // Retrieve the store associated with the current merchant
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.MerchantId == currentMerchantId);

            if (store == null)
            {
                return RedirectToAction("CreateStore"); // أو أي إجراء آخر مناسب
            }

            // Retrieve the products associated with the store
            var products = from p in _context.Products.Where(p => p.StoreId == store.Id) select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString)
                                       || s.Description.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }

        public IActionResult CreateP()
        {
            return View(new AddProductsModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateP(AddProductsModel model)
        {
            // Get the current merchant's ID
            int currentMerchantId = GetCurrentMerchantId();

            // Retrieve the store associated with the current merchant
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.MerchantId == currentMerchantId);

            if (store == null)
            {
                return RedirectToAction("CreateStore"); // أو أي إجراء آخر مناسب
            }

            // Check if the merchant has reached the product limit
            if (store.CurrentProductCount >= store.ProductCount)
            {
                ModelState.AddModelError("", $"لقد تجاوزت الحد الأقصى لعدد المنتجات المسموح بها ({store.ProductCount} منتجات).");
                return View(model); // Return to the create view with validation errors
            }

            if (ModelState.IsValid)
            {
                // Create a new Product object
                var product = new Product();

                // Process the image upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var filePath = Path.Combine(environment.WebRootPath, "products", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    product.Image = fileName; // Save the file name to the database
                }

                // Transfer data from the ViewModel to the Product model
                product.Id = Guid.NewGuid().ToString();
                product.StoreId = store.Id;
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.QuantityInStock = model.QuantityInStock;
                product.Category = model.Category;
                product.Discount = model.Discount; // Set the discount property
                product.HaveDiscount = model.HaveDiscount;


                // Calculate the discounted price
                if (product.HaveDiscount = true)
                {
                    if (product.Discount.HasValue)
                    {
                        product.DiscountedPrice = product.Price * (1 - (decimal)product.Discount / 100);
                    }
                    else
                    {
                        product.DiscountedPrice = null;
                        product.HaveDiscount = false;
                    }
                }
                else
                {
                    product.DiscountedPrice = null;

                }

                _context.Products.Add(product);
                store.CurrentProductCount++;
                _context.Stores.Update(store);
                await _context.SaveChangesAsync();

                return RedirectToAction("productmanagement", "Trader");
            }

            // If model is not valid, return to the create view with validation errors
            return View(model);
        }



        public async Task<IActionResult> EditP(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);


            if (product == null)
            {
                return NotFound();
            }

            var editProductModel = new AddProductsModel
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                QuantityInStock = product.QuantityInStock,
                Category = product.Category,
                Discount = product.Discount
            };

            ViewBag.ProductId = id; 
            ViewData["Image"] = product.Image; 

            return View(editProductModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditP(string id, AddProductsModel addProductsModel)
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

            if (!ModelState.IsValid)
            {
                ViewData["Image"] = product.Image;
                return View(addProductsModel);
            }

            try
            {
                //var originalCategoryId = product.CategoryId;

                // تحديث صور الشعار والغطاء
                if (addProductsModel.ImageFile != null)
                {
                    // حذف الصورة القديمة
                    string oldimgPath = Path.Combine(environment.WebRootPath, "products", product.Image);
                    if (System.IO.File.Exists(oldimgPath))
                    {
                        System.IO.File.Delete(oldimgPath);
                    }

                    // حفظ الصورة الجديدة
                    string imgFileName = Guid.NewGuid().ToString() + Path.GetExtension(addProductsModel.ImageFile.FileName);
                    string imgPath = Path.Combine(environment.WebRootPath, "products", imgFileName);
                    using (var stream = new FileStream(imgPath, FileMode.Create))
                    {
                        await addProductsModel.ImageFile.CopyToAsync(stream);
                        product.Image = imgFileName;
                    }
                }
               

                // تحديث خصائص المتجر
                product.Name = addProductsModel.Name;
                product.Description = addProductsModel.Description;
                product.Price = addProductsModel.Price;
                product.Discount = addProductsModel.Discount;
                product.QuantityInStock = addProductsModel.QuantityInStock;
                product.Category = addProductsModel.Category;
                product.HaveDiscount = addProductsModel.HaveDiscount;

                if(product.HaveDiscount = true)
                {
                    if (product.Discount.HasValue)
                    {
                        product.DiscountedPrice = product.Price * (1 - (decimal)product.Discount / 100);
                    }
                    else
                    {
                        product.DiscountedPrice = null;
                        product.HaveDiscount = false;
                    }
                }
                else
                {
                    product.DiscountedPrice = null;

                }

                _context.Products.Update(product);

                // update category
                //if (originalCategoryId != addProductsModel.CategoryId)
                //{
                //    var oldCategory = await _context.Categories.FindAsync(originalCategoryId);
                //    if (oldCategory != null)
                //    {
                //        _context.Entry(oldCategory).State = EntityState.Modified;

                //        if (oldCategory.StoreCount > 0)
                //            oldCategory.StoreCount--;
                //    }

                //    var newCategory = await _context.Categories.FindAsync(addProductsModel.CategoryId);
                //    if (newCategory != null)
                //    {
                //        _context.Entry(newCategory).State = EntityState.Modified;

                //        newCategory.StoreCount++;

                //    }
                //}
                await _context.SaveChangesAsync(); // استخدام async/await

                return RedirectToAction("productmanagement", "Trader");
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل المنتج.");
                ViewData["Image"] = product.Image;
                return View(addProductsModel);
            }
        }

        // GET: Trader/DeleteP/5
        public async Task<IActionResult> DeleteP(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Trader/DeleteP/5
        [HttpPost, ActionName("DeleteP")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedP(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);

                // Get the current merchant's ID
                int currentMerchantId = GetCurrentMerchantId();

                // Retrieve the store associated with the current merchant
                var store = await _context.Stores.FirstOrDefaultAsync(s => s.MerchantId == currentMerchantId);

                if (store != null)
                {
                    store.CurrentProductCount--; // Decrement the current product count for the store
                    _context.Stores.Update(store); // Update the store in the context
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProductManagement));
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }



        private int GetCurrentMerchantId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var merchant = _context.Merchants.FirstOrDefault(m => m.UserId == userId);
            return merchant?.Id ?? 0; // Return 0 if merchant is not found
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
                var Merchant = await _context.Merchants
                    .FirstOrDefaultAsync(d => d.UserId == user.Id); // Assuming UserId is a foreign key in DeliveryDrivers

                if (Merchant != null)
                {
                    Merchant.MerchantName = model.FirstName + " " + model.LastName; // You can also customize this logic if needed
                    Merchant.MerchantPhone = model.PhoneNumber;

                    await _context.SaveChangesAsync();
                }
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
        public IActionResult reports()
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

            return View(viewModel);
        }

        public async Task<IActionResult> storesettings(string id)
        {
            // الحصول على معرّف التاجر الحالي باستخدام ASP.NET Identity
            int currentMerchantId = 0;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.UserId == userId);
                if (merchant != null)
                {
                    currentMerchantId = merchant.Id;
                }
            }

            if (currentMerchantId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // إذا لم يتم تمرير معرّف المتجر، ابحث عن متجر التاجر
            if (string.IsNullOrEmpty(id))
            {
                var store = await _context.Stores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.MerchantId == currentMerchantId);
                if (store == null)
                {
                    return RedirectToAction("CreateStore", "Trader");
                }
                id = store.Id;
            }

            // جلب تفاصيل المتجر
            var storeDetails = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (storeDetails == null)
            {
                return NotFound();
            }

            // التحقق من أن المتجر ينتمي للتاجر الحالي
            if (storeDetails.MerchantId != currentMerchantId)
            {
                return Forbid();
            }

            // إعداد قائمة الفئات للعرض في القائمة المنسدلة
            ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", storeDetails.CategoryId);

            // إنشاء نموذج لتحرير المتجر
            var editStoreModel = new EditStoreModel
            {
                Id = storeDetails.Id,
                Name = storeDetails.Name,
                Description = storeDetails.Description,
                Location = storeDetails.Location,
                CategoryId = storeDetails.CategoryId
            };

            ViewData["Logo"] = storeDetails.Logo;
            ViewData["Cover"] = storeDetails.CoverImage;

            return View(editStoreModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> storesettings(string id, EditStoreModel editStoreModel)
        {
            // الحصول على معرّف التاجر الحالي
            int currentMerchantId = 0;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.UserId == userId);
                if (merchant != null)
                {
                    currentMerchantId = merchant.Id;
                }
            }

            if (currentMerchantId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // إذا لم يتم تمرير معرّف المتجر، ابحث عنه
            if (string.IsNullOrEmpty(id))
            {
                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.MerchantId == currentMerchantId);
                if (store == null)
                {
                    return NotFound();
                }
                id = store.Id;
                editStoreModel.Id = id;
            }

            // جلب المتجر المراد تحديثه
            var storeToUpdate = await _context.Stores.FindAsync(id);
            if (storeToUpdate == null)
            {
                return NotFound();
            }

            // التحقق من ملكية المتجر
            if (storeToUpdate.MerchantId != currentMerchantId)
            {
                return Forbid();
            }

            // التحقق من صحة النموذج
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", editStoreModel.CategoryId);
                ViewData["Logo"] = storeToUpdate.Logo;
                ViewData["Cover"] = storeToUpdate.CoverImage;
                return View(editStoreModel);
            }

            try
            {
                var originalCategoryId = storeToUpdate.CategoryId;

                // تحديث صورة الشعار إذا تم رفع صورة جديدة
                if (editStoreModel.Logo != null)
                {
                    if (!string.IsNullOrEmpty(storeToUpdate.Logo))
                    {
                        string oldLogoPath = Path.Combine(environment.WebRootPath, "logos", storeToUpdate.Logo);
                        if (System.IO.File.Exists(oldLogoPath))
                        {
                            System.IO.File.Delete(oldLogoPath);
                        }
                    }
                    string logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(editStoreModel.Logo.FileName);
                    string logoPath = Path.Combine(environment.WebRootPath, "logos", logoFileName);
                    using (var stream = new FileStream(logoPath, FileMode.Create))
                    {
                        await editStoreModel.Logo.CopyToAsync(stream);
                        storeToUpdate.Logo = logoFileName;
                    }
                }

                // تحديث صورة الغلاف إذا تم رفع صورة جديدة
                if (editStoreModel.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(storeToUpdate.CoverImage))
                    {
                        string oldCoverPath = Path.Combine(environment.WebRootPath, "covers", storeToUpdate.CoverImage);
                        if (System.IO.File.Exists(oldCoverPath))
                        {
                            System.IO.File.Delete(oldCoverPath);
                        }
                    }
                    string coverFileName = Guid.NewGuid().ToString() + Path.GetExtension(editStoreModel.CoverImage.FileName);
                    string coverPath = Path.Combine(environment.WebRootPath, "covers", coverFileName);
                    using (var stream = new FileStream(coverPath, FileMode.Create))
                    {
                        await editStoreModel.CoverImage.CopyToAsync(stream);
                        storeToUpdate.CoverImage = coverFileName;
                    }
                }

                // تحديث بيانات المتجر
                storeToUpdate.Name = editStoreModel.Name;
                storeToUpdate.Description = editStoreModel.Description;
                storeToUpdate.Location = editStoreModel.Location;
                storeToUpdate.CategoryId = editStoreModel.CategoryId;

                _context.Stores.Update(storeToUpdate);

                // تحديث عدد المتاجر في الفئات إذا تغيرت الفئة
                if (originalCategoryId != editStoreModel.CategoryId)
                {
                    var oldCategory = await _context.Categories.FindAsync(originalCategoryId);
                    if (oldCategory != null && oldCategory.StoreCount > 0)
                    {
                        oldCategory.StoreCount--;
                        _context.Entry(oldCategory).State = EntityState.Modified;
                    }

                    var newCategory = await _context.Categories.FindAsync(editStoreModel.CategoryId);
                    if (newCategory != null)
                    {
                        newCategory.StoreCount++;
                        _context.Entry(newCategory).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("storesettings", new { id = storeToUpdate.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث المتجر: " + ex.Message);
                ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", editStoreModel.CategoryId);
                ViewData["Logo"] = storeToUpdate.Logo;
                ViewData["Cover"] = storeToUpdate.CoverImage;
                return View(editStoreModel);
            }
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
