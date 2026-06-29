using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;
using VOSG_NKBD.Services;

var builder = WebApplication.CreateBuilder(args);


var defaultCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=vosg.db"));

builder.Services.AddDefaultIdentity<VOSG_NKBDUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength        = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddTransient<IEmailSender, EmailSender>();

 var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Home/Error");
     app.UseHsts();
}

     app.UseHttpsRedirection();
     app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture      = new RequestCulture   (defaultCulture),
    SupportedCultures          = new List<CultureInfo>{defaultCulture},
    SupportedUICultures        = new List<CultureInfo>{defaultCulture}
});

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

await DatabaseSeed.SeedAsync(app);

app.Run();