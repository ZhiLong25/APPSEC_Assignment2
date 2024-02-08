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

})
.AddEntityFrameworkStores<AuthDbContext>()
.AddSignInManager<SignInManager<Register>>()
.AddDefaultTokenProviders();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});


builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";

});

// Authorize
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBelongToHRDepartment", policy => policy.RequireClaim("Department", "HR"));
    options.AddPolicy("LoggedIn", policy => policy.RequireAssertion(context => true));
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuditServiceModel>();
builder.Services.AddTransient<EmailSender>();


//Force unauthenticated user to login screen
builder.Services.ConfigureApplicationCookie(Config =>
{
	Config.ExpireTimeSpan = TimeSpan.FromMinutes(5);
	Config.LoginPath = "/Login";
	Config.Cookie.HttpOnly = true;
	Config.SlidingExpiration = true;
	Config.Cookie.SameSite = SameSiteMode.Strict;
});

//Session Timeout
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
	options.Cookie.SameSite = SameSiteMode.Strict;

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
app.UseCors("AllowAllOrigins");
app.UseStaticFiles();

// Custom Error Page
app.UseStatusCodePagesWithRedirects("/errors/{0}");


app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenExpirationMiddleware>();


app.MapRazorPages();

app.Run();
