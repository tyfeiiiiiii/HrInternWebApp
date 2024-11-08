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
            });

            // Add cookie-based authentication
            builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
                .AddCookie("MyCookieAuthenticationScheme", options =>
                {
                    options.LoginPath = "/Authentication/Login"; // Set the login path
                    options.AccessDeniedPath = "/Authentication/AccessDenied"; // Optional: Set access denied path
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
            //app.UseAuthentication(); // Enable authentication
            //app.UseAuthorization();  // Enable authorization

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
                    .ConnectionString(@"Server=(localdb)\Local;Database=HRManagementSystem;Trusted_Connection=True;"))
                .Mappings(m =>
                {
                    m.FluentMappings.AddFromAssemblyOf<EmployeeMap>();
                    //m.FluentMappings.AddFromAssemblyOf<LeaveMap>();
                })
                .BuildSessionFactory();

        }


        //public static ISessionFactory CreateSessionFactory()
        //{
        //    try
        //    {
        //                    return Fluently.Configure()
        //        .Database(MsSqlConfiguration.MsSql2012
        //            .ConnectionString(@"Server=(localdb)\Local;Database=HRManagementSystem;Trusted_Connection=True;"))
        //        .Mappings(m =>
        //        {
        //            // Add all mappings in the assembly
        //            m.FluentMappings.AddFromAssemblyOf<EmployeeMap>();
        //            m.FluentMappings.AddFromAssemblyOf<LeaveMap>();
        //        })
        //        .ExposeConfiguration(cfg =>
        //        {
        //            // Optional: Enable SQL logging for easier debugging
        //            cfg.SetProperty("show_sql", "true");
        //            cfg.SetProperty("format_sql", "true");
        //        })
        //        .BuildSessionFactory();

        //    }
        //    catch (FluentNHibernate.Cfg.FluentConfigurationException ex)
        //    {
        //        Console.WriteLine("FluentConfigurationException: " + ex.Message);
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
        //            Console.WriteLine("Inner Exception Type: " + ex.InnerException.GetType().ToString());
        //            Console.WriteLine("Stack Trace: " + ex.InnerException.StackTrace);
        //        }
        //        else
        //        {
        //            Console.WriteLine("No Inner Exception Details Available.");
        //        }
        //        throw; // Re-throw to stop the application if configuration is invalid
        //    }

        //}

    }
    }
