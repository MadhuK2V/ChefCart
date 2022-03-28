using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using ChefWebAPI.Helpers;
using ChefWebAPI.Middleware;
using ChefWebAPI.Services;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace ChefWebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // add services to the DI container
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddDbContext<DataContext>();
            services.AddCors();
            services.AddMvc(options =>
            {
                ////options.Filters.Add(typeof(DataEncriptionAddCookie
                options.Filters.Add(typeof(UnhandledExceptionFilter));
               
            });
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen();

            // configure strongly typed settings object
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddAuthentication()
                .AddCookie()
                .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                facebookOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });
            //// "AllowedHosts": "localhost;127.0.0.1;[::1]"
            //var hosts = Configuration["AllowedHosts"]?
            //                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            //if (hosts?.Length > 0)
            //{
            //    services.Configure<HostFilteringOptions>(
            //        options => options.AllowedHosts = hosts);
            //}
            // configure DI for application services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAppVersionService, AppVersionService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<ICategorieService, CategorieService>();
            services.AddScoped<IChildCategoryService, ChildCategoryService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IFilterRangeService, FilterRangeService>();
            services.AddScoped<IFilterTypeService, FilterTypeService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductDetailService, ProductDetailService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ISubCategorieService, SubCategorieService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IVegTypeService, VegTypeService>();
            services.AddScoped<IZipCodeService, ZipCodeService>();
            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IStoreDetailService, StoreDetailService>();
            services.AddScoped<IFavoriteStoreDetailService, FavoriteStoreDetailService>();

            services.AddSwaggerGen(x =>
            {
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[0] }
                };

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization Header using the bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                x.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            new List<string>()
                        }
                    });
            });
        }

        // configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // migrate database changes on startup (includes initial db creation)
            context.Database.Migrate();

            // generated swagger json and swagger ui middleware
            app.UseSwagger();
            app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET Core Sign-up and Verification API"));

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(x => x.MapControllers());
            app.UseHostFiltering();
        }
    }
}
