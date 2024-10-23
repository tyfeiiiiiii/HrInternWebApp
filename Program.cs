using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using HrInternWebApp.Models.Map;
using HrInternWebApp.Services;  

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

            // NHibernate session factory setup
            var sessionFactory = CreateSessionFactory();
            builder.Services.AddSingleton<ISessionFactory>(sessionFactory);  // Register session factory as singleton

            // NHibernate session management per request
            builder.Services.AddScoped(factory => sessionFactory.OpenSession());  // Inject NHibernate session

            // Register the LeaveService for dependency injection
            builder.Services.AddScoped<LeaveService>();

            // You can register other services like UserService here if needed
            // builder.Services.AddScoped<UserService>();

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
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "login",
                pattern: "{controller=LogIn}/{action=Login}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

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
                    m.FluentMappings.AddFromAssemblyOf<EmployeeMap>();  // Employee Map
                    m.FluentMappings.AddFromAssemblyOf<LeaveMap>();     // Leave Map
                })
                .BuildSessionFactory();
        }
    }
}
