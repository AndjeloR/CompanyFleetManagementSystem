using CompanyFleetManagementSystem.Data;
using CompanyFleetManagementSystem.Models;
using CompanyFleetManagementSystem.Services.Implementations;
using CompanyFleetManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<ITripService, TripService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapRazorPages();

// Ensure database is created and seed data - run async initialization
await InitializeDatabaseAsync(app.Services);

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

// Helper method for async database initialization
async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


        // Delete all existing data and recreate database
        await dbContext.Database.EnsureDeletedAsync();


        await dbContext.Database.EnsureCreatedAsync();
    }

    using (var scope = services.CreateScope())
    {
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Administrator", "Dispatcher", "Driver" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin user
            var adminEmail = "admin@admin.com";
            var adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    RoleName = "Administrator"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Code} - {error.Description}");
                    }
                }
            }

            // Create test dispatcher user
            var dispatcherEmail = "dispatcher@test.com";
            var dispatcherPassword = "Dispatcher123!";
            var dispatcher = await userManager.FindByEmailAsync(dispatcherEmail);

            if (dispatcher == null)
            {
                var dispatcherUser = new ApplicationUser
                {
                    UserName = dispatcherEmail,
                    Email = dispatcherEmail,
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Dispatcher",
                    RoleName = "Dispatcher"
                };

                var result = await userManager.CreateAsync(dispatcherUser, dispatcherPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(dispatcherUser, "Dispatcher");
                }
            }

            // Create test driver user
            var driverEmail = "driver@test.com";
            var driverPassword = "Driver123!";
            var driver = await userManager.FindByEmailAsync(driverEmail);

            if (driver == null)
            {
                var driverUser = new ApplicationUser
                {
                    UserName = driverEmail,
                    Email = driverEmail,
                    EmailConfirmed = true,
                    FirstName = "Bob",
                    LastName = "Driver",
                    RoleName = "Driver"
                };

                var result = await userManager.CreateAsync(driverUser, driverPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(driverUser, "Driver");
                }
            }

            // Create test trips
            if (!dbContext.Trips.Any())
            {
                var driverUser = await userManager.FindByEmailAsync(driverEmail);
                var dispatcherUser = await userManager.FindByEmailAsync(dispatcherEmail);

                var testTrips = new List<Trip>
                {
                    new Trip
                    {
                        Name = "Доставка във София",
                        Description = "Доставка на пакети във София",
                        Route = "София - Пловдив",
                        DepartureTime = DateTime.Now.AddHours(2),
                        ExpectedReturnTime = DateTime.Now.AddHours(8),
                        DriverEmail = driverEmail,
                        DriverId = driverUser.Id,
                        DispatcherId = dispatcherUser.Id,
                        Status = TripStatus.Waiting
                    },
                    new Trip
                    {
                        Name = "Транспорт до Варна",
                        Description = "Превоз на товари до морския град",
                        Route = "София - Варна",
                        DepartureTime = DateTime.Now.AddHours(4),
                        ExpectedReturnTime = DateTime.Now.AddHours(12),
                        DriverEmail = driverEmail,
                        DriverId = driverUser.Id,
                        DispatcherId = dispatcherUser.Id,
                        Status = TripStatus.Waiting
                    }
                };

                dbContext.Trips.AddRange(testTrips);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Test trips created successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}