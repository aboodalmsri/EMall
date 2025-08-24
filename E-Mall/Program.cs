using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_Mall.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("EDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'EDbContextConnection' not found.");

builder.Services.AddDbContext<EDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<E_MallUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "customerRoute",
    pattern: "Customer/{action=Profile}/{id?}",
    defaults: new { controller = "Customer" }
);


app.MapControllerRoute(
    name: "adminRoute",
    pattern: "Admin/{action}",
    defaults: new { controller = "Admin" }
);

app.MapControllerRoute(
    name: "StoreDetails",
    pattern: "StoreDetails/{id}",
    defaults: new { controller = "Customer", action = "StoreDetails" }
);

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var RoleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Customer", "Delivery", "Trader" };
    foreach (var role in roles)
    {
        if (!await RoleManager.RoleExistsAsync(role))
            await RoleManager.CreateAsync(new IdentityRole(role));
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<E_MallUser>>();
    string email = "AdminEMall@Admin.com";
    string password = "Test@1234";


    if (await userManager.FindByEmailAsync(email) == null)
    {

        var user = new E_MallUser();

        user.UserName = email;
        user.Email = email;
        user.EmailConfirmed = true;
        user.FirstName = "Admin";
        user.LastName = "User";
        user.PhoneNumber = "999999999";
        await userManager.CreateAsync(user, password);

        await userManager.AddToRoleAsync(user, "Admin");


    }
}

app.Run();