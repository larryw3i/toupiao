using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using toupiao.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using toupiao.Services;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

namespace toupiao
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add 
        // services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddMvc()
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = 
                        (type, factory) =>
                            factory.Create(typeof(Program));
                })
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix);


            services.AddLocalization(options => 
                options.ResourcesPath = "Localization");

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("PostgreSQLConnection")));
            
            services.AddDefaultIdentity<IdentityUser>(
                    // 验证用户才能登录
                    options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(
                options =>
                {
                // Default Password settings.
                options.Password.RequireDigit = true;
                // 需要小写
                options.Password.RequireLowercase = true;
                // 需要符号
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                // 不能全部相同
                options.Password.RequiredUniqueChars = 1;
                });


            services.Configure<IdentityOptions>(options =>
            {
                // Default User settings.
                // 允许用户名为中文
                options.User.AllowedUserNameCharacters = null;
                // 邮箱的唯一性
                options.User.RequireUniqueEmail = false;

            });

            services.Configure<DataProtectionTokenProviderOptions>(
                options => options.TokenLifespan = TimeSpan.FromHours(3));



            services.Configure<RequestLocalizationOptions>(options =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("zh-Hans"),
                        new CultureInfo("en-US"),
                    };

                    options.DefaultRequestCulture = 
                        new RequestCulture("zh-Hans");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.RequestCultureProviders =  
                        new List<IRequestCultureProvider>{  
                            new ToupiaoRequestCultureProvider() };
                });

            
            services.AddControllersWithViews();
            
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure
        //  the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change
                //  this for production scenarios, 
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy();

            app.UseRequestLocalization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "ZArea",
                    pattern:
                    "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
