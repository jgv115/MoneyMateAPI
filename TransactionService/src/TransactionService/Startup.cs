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
using TransactionService.Domain.Services.Categories;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Domain.Services.PayerPayees;
using TransactionService.Domain.Services.Profiles;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Helpers;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Profiles;
using TransactionService.Repositories;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Profiles;
using TransactionService.Services;
using TransactionService.Services.Initialisation.CategoryInitialisation;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Options;

namespace TransactionService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

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
            services
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                .AddControllersWithViews(options => options.InputFormatters.Insert(0, GetJsonPatchInputFormatter()));
            services.AddControllersWithViews().AddNewtonsoftJson();

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

            services.AddHttpClient<IPayerPayeeEnricher, GooglePlacesPayerPayeeEnricher>();
            services.Configure<GooglePlaceApiOptions>(options =>
                Configuration.GetSection("GooglePlaceApi").Bind(options));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            // app.UseHttpsRedirection();

            app.UseRouting();

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