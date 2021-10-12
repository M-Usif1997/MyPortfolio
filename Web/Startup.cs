using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Settings;
using IdentityModel;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.mapper;

namespace Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("MyPortfolioDB"));
            });

            //Add Identity
            services.AddIdentity<Owner, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            })

            .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();


            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AccountProfile());
            });

            //Configure Mapper
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            //only authenticated user can Enter Application
            services.AddMvcCore(options =>
            {
              var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter (policy));
            }).AddXmlSerializerFormatters();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();


            //External Login Providers authentication
            services.AddAuthentication()
                     .AddGoogle("google",options =>
                     {
                         var googleAuth = configuration.GetSection("Authentication:Google");
                         options.ClientId = googleAuth["ClientId"];
                         options.ClientSecret = googleAuth["ClientSecret"];
                         options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                         options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");


                     })
                    .AddFacebook("facebook",options =>
                    {
                        var facebookAuth = configuration.GetSection("Authentication:Facebook");
                        options.AppId = facebookAuth["AppId"];
                        options.AppSecret = facebookAuth["AppSecret"];
                  
                    });



            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>)) ;
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IMailingService, MailingService>();
            //to open Session for Login User
            services.AddSession();


            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        }


       
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            //to open Session for Login User
            app.UseSession();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "defaultRoute",
                    "{controller=Account}/{action=login}/{id?}"
                    );
            });



        }
    }
}
