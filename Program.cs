using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using HrInternWebApp.Models.Maps;

namespace HrInternWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add session and session timeout configurations
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Enforce HTTPS
            });

            // Add cookie-based authentication
            builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
                .AddCookie("MyCookieAuthenticationScheme", options =>
                {
                    options.LoginPath = "/Authentication/Login"; 
                    //options.AccessDeniedPath = "/Authentication/AccessDenied"; // Optional: Set access denied path
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
                });

            // Add authorization policies if needed
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            });

            // NHibernate session factory setup
            var sessionFactory = CreateSessionFactory();
            builder.Services.AddSingleton<ISessionFactory>(sessionFactory);  // Register session factory as singleton

            // NHibernate session management per request
            builder.Services.AddScoped(factory => sessionFactory.OpenSession());  // Inject NHibernate session

            // Register the LeaveService for dependency injection
            builder.Services.AddScoped<LeaveService>();
            builder.Services.AddScoped<EmployeeService>();
            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication(); 
            app.UseAuthorization(); 

            app.MapControllerRoute(
                name: "login",
                pattern: "{controller=Authentication}/{action=Login}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "signup",
                pattern: "{controller=Authentication}/{action=Signup}");
            app.Run();
        }

        // Fluent NHibernate configuration for the session factory
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                    .ConnectionString(@"Server=(localdb)\MSSQLLocalDB;Database=HRManagementSystem;Trusted_Connection=True;"))
                .Mappings(m =>
                {
                    //no need to add on each map file as the n hibernat will combine tgt since th einitial file that stated here
                    m.FluentMappings.AddFromAssemblyOf<EmployeeMap>();
                })
                .BuildSessionFactory();

        }
    }
    }
