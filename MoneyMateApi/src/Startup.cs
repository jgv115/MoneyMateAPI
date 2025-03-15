using System;
using System.Linq;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Lambda;
using Amazon.Runtime;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using MoneyMateApi.Connectors.GooglePlaces;
using MoneyMateApi.Connectors.GooglePlaces.Options;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Services.Categories;
using MoneyMateApi.Domain.Services.Categories.UpdateCategoryOperations;
using MoneyMateApi.Domain.Services.PayerPayees;
using MoneyMateApi.Domain.Services.Profiles;
using MoneyMateApi.Domain.Services.Tags;
using MoneyMateApi.Domain.Services.Transactions;
using MoneyMateApi.Helpers;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Profiles;
using MoneyMateApi.Repositories;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Profiles;
using MoneyMateApi.Services;
using MoneyMateApi.Services.Initialisation.CategoryInitialisation;
using MoneyMateApi.Services.PayerPayeeEnricher;

namespace MoneyMateApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public static IConfiguration Configuration { get; private set; }
        public IHostEnvironment HostEnvironment { get; set; }

        // We still rely on Newtonsoft.Json for JSON Patch inputs. All other serialisation + deserialisation is done by System.Text.Json
        // https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0#add-support-for-json-patch-when-using-systemtextjson
        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (HostEnvironment.EnvironmentName != "dev")
                services.AddCors(options =>
                {
                    var corsAllowedOrigin = Configuration.GetValue<string>("CorsConfig:AllowedOrigin");

                    if (!string.IsNullOrEmpty(corsAllowedOrigin))
                        options.AddPolicy(CorsPolicy.MoneyMateFrontEndPolicy,
                            builder =>
                            {
                                builder
                                    .WithOrigins(corsAllowedOrigin)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                            });
                });

            // TODO: fix fluent validation
            services
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                .AddControllersWithViews(options => options.InputFormatters.Insert(0, GetJsonPatchInputFormatter()));

            var auth0Settings = Configuration.GetSection("Auth0");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = auth0Settings.GetSection("Authority").Value;
                options.Audience = auth0Settings.GetSection("Audience").Value;
                options.SaveToken = true;
            });

            services.AddMemoryCache();
            services.AddSingleton<ISystemClock, SystemClock>();
            services.AddSingleton<ITimePeriodHelper, TimePeriodHelper>();

            services.AddScoped<CurrentUserContext>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ITransactionHelperService, TransactionHelperService>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<IUpdateCategoryOperationFactory, UpdateCategoryOperationFactory>();
            services.AddScoped<IPayerPayeeService, PayerPayeeService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<ICategoryInitialiser, LambdaCategoryInitialiser>();
            services.AddScoped<ITagService, TagService>();

            services.AddHttpClient<IGooglePlacesConnector, GooglePlacesConnector>();
            services.AddScoped<IPayerPayeeEnricher, GooglePlacesPayerPayeeEnricher>();
            
            services.Configure<GooglePlaceApiOptions>(options =>
                Configuration.GetSection("GooglePlaceApi").Bind(options));

            services.AddAutoMapper(typeof(TransactionProfile), typeof(CategoryProfile), typeof(CategoryEntityProfile));

            var awsSettings = Configuration.GetSection("AWS");
            var awsLocalMode = awsSettings.GetValue<bool>("LocalMode");

            if (awsLocalMode)
            {
                var awsServiceUrl = awsSettings.GetValue<string>("ServiceUrl");
                var awsRegion = awsSettings.GetValue<string>("Region");
                var awsKey = awsSettings.GetValue<string>("AccessKey");
                var awsSecret = awsSettings.GetValue<string>("SecretKey");

                services.AddSingleton<IAmazonLambda, MockAmazonLambdaClient>();
            }
            else
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().Build();
                AWSOptions awsOptions = configuration.GetAWSOptions();
                awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
                services.AddDefaultAWSOptions(awsOptions);

                services.AddAWSService<IAmazonLambda>();
            }

            var cockroachDbConfigSection = Configuration.GetSection("CockroachDb");

            var cockroachDbConnectionString =
                cockroachDbConfigSection.GetValue<string>("ConnectionString");

            services.AddSingleton(_ => new DapperContext(cockroachDbConnectionString));

            services.AddScoped<IUserRepository, CockroachDbUserRepository>();
            services.AddScoped<ICategoriesRepository, CockroachDbCategoriesRepository>();
            services.AddScoped<IPayerPayeeRepository, CockroachDbPayerPayeeRepository>();
            services.AddScoped<ITransactionRepository, CockroachDbTransactionRepository>();
            services.AddScoped<IProfilesRepository, CockroachDbProfilesRepository>();
            services.AddScoped<ITagRepository, CockroachDbTagRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("dev"))
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            // app.UseHttpsRedirection();

            app.UseRouting();
            if (!env.IsEnvironment("dev"))
                app.UseCors(CorsPolicy.MoneyMateFrontEndPolicy);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments("/api/health") &&
                           !context.Request.Path.StartsWithSegments("/api/initialisation"),
                builder => builder.UseMiddleware<UserContextMiddleware>());

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health") &&
                                   !context.Request.Path.StartsWithSegments("/api/profiles"),
                builder => builder.UseMiddleware<UserProfileMiddleware>());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}