using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Mall.Controllers
{
    [Authorize(Roles = "Delivery")]
    public class DeliveryController : Controller
    {
        private readonly UserManager<E_MallUser> _userManager;
        private readonly EDbContext _context;


        public DeliveryController(UserManager<E_MallUser> userManager, EDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult availableorders()
        {
            return View();
        }

        public IActionResult dashbord()
        {
            return View();
        }
        public IActionResult orderdetails()
        {
            return View();
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

            // Save the changes to the AspNetUsers table
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Now, update the DeliveryDrivers table as well
                var deliveryDriver = await _context.DeliveryDrivers
                    .FirstOrDefaultAsync(d => d.UserId == user.Id); // Assuming UserId is a foreign key in DeliveryDrivers

                if (deliveryDriver != null)
                {
                    // Update the properties of DeliveryDriver
                    deliveryDriver.DriverName = model.FirstName + " " + model.LastName; // You can also customize this logic if needed
                    deliveryDriver.DriverPhone = model.PhoneNumber;

                    // Save the changes to the DeliveryDrivers table
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

        public IActionResult reviews()
        {
            return View();
        }
        public IActionResult shops()
        {
            return View();
        }
        public IActionResult support()
        {
            return View();
        }
        public IActionResult trackroute()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
