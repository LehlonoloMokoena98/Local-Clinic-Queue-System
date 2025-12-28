// ====================================================================
// CLINIC QUEUE SYSTEM - MAIN APPLICATION CONFIGURATION
// ====================================================================
// This file configures the ASP.NET Core application with all required services
// Including database connection, SignalR for real-time updates, and session management

using ClinicQueueSystem.Data;
using ClinicQueueSystem.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// SERVICE CONFIGURATION
// ====================================================================

// Add MVC Controllers and Views support
// Enables the Model-View-Controller pattern for web pages
builder.Services.AddControllersWithViews();

// Add SignalR service for real-time communication
// Enables live queue updates without page refresh
builder.Services.AddSignalR();

// Configure Entity Framework with SQL Server database
// Connection string from appsettings.json for local SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session support for admin authentication
// Enables storing admin login status across requests
builder.Services.AddSession();

var app = builder.Build();

// ====================================================================
// REQUEST PIPELINE CONFIGURATION
// ====================================================================

// Configure error handling for production environment
if (!app.Environment.IsDevelopment())
{
    // Use custom error page for production
    app.UseExceptionHandler("/Home/Error");
    // Enable HTTP Strict Transport Security (HSTS)
    app.UseHsts();
}

// Enable serving static files (CSS, JS, images)
app.UseStaticFiles();

// Enable routing for controllers and actions
app.UseRouting();

// Enable authentication middleware
app.UseAuthentication();

// Enable authorization middleware  
app.UseAuthorization();

// Enable session middleware for admin login state
app.UseSession();

// Configure default route pattern
// Routes requests to Admin/Login by default (secure entry point)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

// Map SignalR hub for real-time queue updates
// Enables WebSocket connection at /queueHub endpoint
app.MapHub<QueueHub>("/queueHub");

// Start the application
app.Run();
