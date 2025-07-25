using EShiftSystem.Data;
using EShiftSystem.Models;
using EShiftSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

// create web application builder
var builder = WebApplication.CreateBuilder(args);

// configure database connection and entity framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// configure identity system with roles and email confirmation
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.SignIn.RequireConfirmedEmail = false; 
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// register custom services for dependency injection
builder.Services.AddScoped<IJobNumberGenerator, JobNumberGenerator>();

//// register email services
//builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

// add mvc controllers and views support
builder.Services.AddControllersWithViews();

// build the application
var app = builder.Build();

// configure middleware pipeline for different environments
if (app.Environment.IsDevelopment())
{
    //shows detailed migration errors for developers
    app.UseMigrationsEndPoint();
}
else
{
    //friendly error page for users
    app.UseExceptionHandler("/Home/Error");
    // the default HSTS value is 30 days.
    app.UseHsts();
}

// configure request processing pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// configure area-based routing for admin and customer sections
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// configure default routing for main controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // apply any pending database migrations automatically
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    
    await SeedData.Initialize(services);
}

// enable razor pages support
app.MapRazorPages();

// start the application
app.Run();
