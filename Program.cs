using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Data;
using SimpleCRM.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crm.db"));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add custom services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log the error (in production, use proper logging)
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Add this before Authorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}") // Changed default to Login
    .WithStaticAssets();


app.Run();
