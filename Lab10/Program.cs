using Lab10.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lab10.Models;



namespace Lab10
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                //options.Cookie.Name = ".TeaShop.Session";
                options.Cookie.IsEssential = true;
            }
            );
    

            builder.Services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TeaShopDatabase"))

            );
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                  policy.RequireRole("Admin"));

            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("UserPolicy", policy =>
                {
                    policy.RequireAssertion(context =>
                         !context.User.IsInRole("Admin"));
                });
            });

            //api

            //builder.Services.AddSingleton<IRepository, ModelRepository>();
            builder.Services.AddScoped<IRepository<Article>, ModelRepository<Article>>();
            builder.Services.AddScoped<IRepository<Category>, ModelRepository<Category>>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            using (var scope = app.Services.CreateScope())
            {

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                MyIdentityDataInitializer.SeedData(userManager, roleManager);
            }

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

    }

}