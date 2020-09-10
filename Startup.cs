using System;
using BlogHost.BackgroundService;
using BlogHost.Data;
using BlogHost.Data.Interfaces;
using BlogHost.Data.Models;
using BlogHost.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRApp;

namespace BlogHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set;}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>(opts => 
            {
                opts.Password.RequiredLength = 5;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDBContext>()
            .AddDefaultTokenProviders();

            services.Configure<CookiePolicyOptions>(options =>//???
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
            });

            services.AddTransient<IUser, UserRepository>();
            services.AddTransient<IPublication, PublicationRepository>();
            services.AddTransient<ITopic, TopicRepository>();
            services.AddTransient<IComment, CommentRepository>();

            services.AddControllersWithViews();

            services.AddSignalR();

            services.AddHostedService<TimedHostedService>();

            services.Configure<ForwardedHeadersOptions>(options =>//////////////////////////////////////////////
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public void Configure(IApplicationBuilder app, IConfiguration Configuration)
        {
            app.UseForwardedHeaders();

            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

           
            ///если возникает ошибка, то перенаправит на error  и выведит ошибку
            app.Map("/error", ap => ap.Run(async context =>
            {
                await context.Response.WriteAsync($"Err: {context.Request.Query["code"]}");
            }));

            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });

            app.UseEndpoints(endpoints =>
            {
                //опеделение маршрутов 
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");//шаблон маршрута, с которыми будуь сопоставляться входящие маршруты
                endpoints.MapHub<ChatHub>("/Chat");//сопоставления определенного маршрута с определенным хабом
            });
        }
    }
}
