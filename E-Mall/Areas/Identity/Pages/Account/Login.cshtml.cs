using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Mall.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace E_Mall.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<E_MallUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<E_MallUser> _userManager;
        private readonly EDbContext _context;


        public LoginModel(SignInManager<E_MallUser> signInManager, ILogger<LoginModel> logger, UserManager<E_MallUser> userManager, EDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user1 = await _userManager.FindByEmailAsync(Input.Email);
                if (user1 != null)
                {
                    var deliveryDriver = await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.DriverEmail == Input.Email);
                    if (deliveryDriver != null && !deliveryDriver.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "هذا الحساب غير نشط. يرجى الاتصال بالمسؤول.");
                        return Page();
                    }

                    var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.MerchantEmail == Input.Email);
                    if (merchant != null && !merchant.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "هذا الحساب غير نشط. يرجى الاتصال بالمسؤول.");
                        return Page();
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Get the user
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    // Check the user's roles and redirect accordingly
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Dashbord", "Admin"); // Redirect to Admin controller
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Customer"))
                    {
                        return RedirectToAction("Dashbord", "Customer"); // Redirect to Customer controller
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Delivery"))
                    {
                        return RedirectToAction("Dashbord", "Delivery"); // Redirect to Delivery controller
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Trader"))
                    {
                        return RedirectToAction("Dashbord", "Trader"); // Redirect to Trader controller
                    }
                    else
                    {
                        return LocalRedirect(returnUrl); // Fallback: redirect to the return URL or Home page
                    }
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}