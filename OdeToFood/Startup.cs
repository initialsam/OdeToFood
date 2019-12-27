using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Utility.Email;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdeToFood.Core;
using OdeToFood.Data;
using OdeToFood.Infrastructure;

namespace OdeToFood
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<OdeToFoodDbContext>(options =>
            {
                //options.UseSqlServer(Configuration.GetConnectionString("OdeToFoodDb"));
                options.UseSqlite(Configuration.GetConnectionString("OdeToFoodSqlite"));
            });
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddTransient<IEmailSender, IdentityEmailSender>();
            services.AddScoped<IRestaurantData, SqlRestaurantData>();
            //services.AddDefaultIdentity<OdeToFoodUser>
            services.AddIdentity<OdeToFoodUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;//�����n����Email

                options.Password.RequiredLength = 3;//�ֱ̤K�X����
                options.Password.RequireDigit = false;//�ݭn�Ʀr
                options.Password.RequireNonAlphanumeric = false;//�ݭn�D�^�ƪ��r�� ex:@
                options.Password.RequireUppercase = false;//�ݭn�j�g�r��
                options.Password.RequiredUniqueChars = 3; //�ܤ֭n���Ӧr�����@��

                options.User.RequireUniqueEmail = true;//Email���୫��

                options.Lockout.MaxFailedAccessAttempts = 3;//3���K�X�~�N��w����
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);//��w1����
                options.Lockout.AllowedForNewUsers = true;//�s�W���ϥΪ̤]�|�Q��w
            })
            .AddEntityFrameworkStores<OdeToFoodDbContext>()
            //.AddDefaultUI()
            .AddDefaultTokenProviders();

            services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = "SuccessReName";
                o.ExpireTimeSpan = TimeSpan.FromDays(365);
            });

            services.AddRazorPages();
            services.AddControllersWithViews();
            /*
            services.AddDbContext<OdeToFoodContext>(options =>
                     options.UseSqlite(
                         context.Configuration.GetConnectionString("OdeToFoodContextConnection")));

            services.AddDefaultIdentity<OdeToFoodUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<OdeToFoodContext>();
                */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use(SayHelloMiddleware);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseNodeModules();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
        private RequestDelegate SayHelloMiddleware(
                                  RequestDelegate next)
        {
            return async ctx =>
                         {

                             if (ctx.Request.Path.StartsWithSegments("/hello"))
                             {
                                 await ctx.Response.WriteAsync("Hello, World!");
                             }
                             else
                             {
                                 await next(ctx);
                             }
                         };
        }
    }
}
