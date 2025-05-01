using Business.Factories;
using Business.Interfaces;
using Business.Services;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services;
using WebApp.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
// 5. MVC
builder.Services.AddControllersWithViews();

// 1. Add DB Context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity config
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// 3. Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/SignIn";
    options.AccessDeniedPath = "/Auth/Denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => !context.Request.Cookies.ContainsKey("cookieConsent");
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// 4. Authentication Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

// 5. Authorization Policies
builder.Services.AddAuthorizationBuilder()
                                // 4. Authorization Policies
                                .AddPolicy("Admins", policy => policy.RequireRole("Admin"))
                                // 4. Authorization Policies
                                .AddPolicy("Users", policy => policy.RequireRole("Admin", "User"));



// 6. SignalR
builder.Services.AddSignalR();

// 7. Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


// 8. Repositories
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


// 9. Factories
builder.Services.AddScoped<MemberFactory>();
builder.Services.AddScoped<UserFactory>();
builder.Services.AddScoped<ClientFactory>();
builder.Services.AddScoped<ProjectFactory>();

// 10. Build app
var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// 11. Seed Roles
async Task SeedRolesAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// 12. Seed Default Admin + Member
async Task SeedDefaultAdminAsync(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var memberRepository = services.GetRequiredService<IMemberRepository>();

    var adminEmail = "admin@domain.com";
    var adminPassword = "BytMig123!";

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator"
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            await userManager.AddToRoleAsync(adminUser, "Admin");

            var member = new MemberEntity
            {
                FirstName = adminUser.FirstName,
                LastName = adminUser.LastName,
                Email = adminUser.Email!,
                UserId = adminUser.Id
            };

            await memberRepository.CreateAsync(member);
            await memberRepository.SaveToDatabaseAsync();
        }
    }
}

// 13. Run seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);
    await SeedDefaultAdminAsync(services);
}

// 14. Routing
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}/{id?}")
    .WithStaticAssets();

// 15. SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();