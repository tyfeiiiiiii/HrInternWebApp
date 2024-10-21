using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using HrInternWebApp.Models;  

namespace HrInternWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
  
            builder.Services.AddControllersWithViews();

 
            builder.Services.AddHttpContextAccessor();


            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Create NHibernate session factory
            var sessionFactory = CreateSessionFactory();
            builder.Services.AddSingleton<ISessionFactory>(sessionFactory);  // Register session factory as a singleton service

            // Add NHibernate session management
            builder.Services.AddScoped(factory => sessionFactory.OpenSession());  // Inject NHibernate sessions

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
                name: "home",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }

        // NHibernate configuration
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                    .ConnectionString(@"Server=(localdb)\Local;Database=HRManagementSystem;Trusted_Connection=True;"))
                .Mappings(m =>
                {
                    m.FluentMappings.AddFromAssemblyOf<EmployeeMap>();  
                    m.FluentMappings.AddFromAssemblyOf<LeaveMap>();    
                })
                .BuildSessionFactory();
        }
    }
}
