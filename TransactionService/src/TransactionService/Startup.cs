using System;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
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
using TransactionService.Domain.Services.Transactions;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Profiles;
using TransactionService.Repositories;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Profiles;
using TransactionService.Repositories.DynamoDb;
using TransactionService.Services;
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
            services.AddScoped<ITransactionHelperService, TransactionHelperService>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<IUpdateCategoryOperationFactory, UpdateCategoryOperationFactory>();
            services.AddScoped<IPayerPayeeService, PayerPayeeService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            services.AddAutoMapper(typeof(TransactionProfile), typeof(CategoryProfile), typeof(CategoryEntityProfile));

            var awsSettings = Configuration.GetSection("AWS");
            var awsLocalMode = awsSettings.GetValue<bool>("LocalMode");

            if (awsLocalMode)
            {
                var awsServiceUrl = awsSettings.GetValue<string>("ServiceUrl");
                var awsRegion = awsSettings.GetValue<string>("Region");
                var awsKey = awsSettings.GetValue<string>("AccessKey");
                var awsSecret = awsSettings.GetValue<string>("SecretKey");

                services.AddSingleton<IAmazonDynamoDB>(_ =>
                {
                    var clientConfig = new AmazonDynamoDBConfig
                    {
                        ServiceURL = awsServiceUrl,
                        AuthenticationRegion = awsRegion
                    };
                    return new AmazonDynamoDBClient(awsKey, awsSecret, clientConfig);
                });
            }
            else
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().Build();
                AWSOptions awsOptions = configuration.GetAWSOptions();
                awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
                services.AddDefaultAWSOptions(awsOptions);
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            services.AddSingleton(new DynamoDbRepositoryConfig
            {
                TableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
            });

            var cockroachDbConfigSection = Configuration.GetSection("CockroachDb");
            var cockroachDbFeatureEnabled = cockroachDbConfigSection.GetValue<string>("Enabled");

            if (!string.IsNullOrEmpty(cockroachDbFeatureEnabled) && bool.Parse(cockroachDbFeatureEnabled))
            {
                var cockroachDbConnectionString =
                    cockroachDbConfigSection.GetValue<string>("ConnectionString");

                services.AddSingleton(_ => new DapperContext(cockroachDbConnectionString));

                services.AddScoped<ICategoriesRepository, CockroachDbCategoriesRepository>();
                services.AddScoped<IPayerPayeeRepository, CockroachDbPayerPayeeRepository>();
            }
            else
            {
                services.AddScoped<ICategoriesRepository, DynamoDbCategoriesRepository>();
                services.AddScoped<IPayerPayeeRepository, DynamoDbPayerPayeeRepository>();
            }

            services.AddScoped<ITransactionRepository, DynamoDbTransactionRepository>();

            services.AddHttpClient<IPayerPayeeEnricher, GooglePlacesPayerPayeeEnricher>();
            services.Configure<GooglePlaceApiOptions>(options =>
                Configuration.GetSection("GooglePlaceApi").Bind(options));

            Console.WriteLine(((IConfigurationRoot) Configuration).GetDebugView());
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

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health"),
                builder => builder.UseMiddleware<UserContextMiddleware>());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}