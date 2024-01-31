using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.Pages;
using APPSEC_Assignment2.ViewModels; // Import your Register class
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Win32;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<Register, IdentityRole>(options =>
{
    // Account lockout settings
    options.Lockout.MaxFailedAccessAttempts = 3; // Maximum number of failed attempts before lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Lockout duration
    options.Lockout.AllowedForNewUsers = true; // Allow lockout for new users


    // Enable 2FA
    options.SignIn.RequireConfirmedAccount = true;

})
.AddEntityFrameworkStores<AuthDbContext>()
.AddSignInManager<SignInManager<Register>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Use the default scheme
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyCookieAuth";
        options.LoginPath = "/Login"; // Specify the login page
        options.LogoutPath = "/Logout"; // Specify the logout page
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuditServiceModel>();
builder.Services.AddTransient<EmailSender>();


//Force unauthenticated user to login screen
builder.Services.ConfigureApplicationCookie(Config =>
{
    Config.LoginPath = "/Login";
});

//Session Timeout
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Email;
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.Email;
    options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

// Custom Error Page
app.UseStatusCodePagesWithRedirects("/errors/{0}");


app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenExpirationMiddleware>();

// Custom Error Page
//app.UseEndpoints(endpoints =>
//{

//    endpoints.MapRazorPages();
//    endpoints.MapFallbackToPage("/Error");
//});


app.MapRazorPages();

app.Run();
