using System;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.Categories;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Profiles;
using TransactionService.Repositories;
using TransactionService.Services;

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
            Console.WriteLine(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            Console.WriteLine(auth0Settings.GetSection("Authority").Value);
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

            services.AddAutoMapper(typeof(TransactionProfile), typeof(CategoryProfile));

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

            services.AddSingleton<ITransactionRepository, DynamoDbTransactionRepository>();
            services.AddScoped<ICategoriesRepository, DynamoDbCategoriesRepository>();
            services.AddSingleton<IPayerPayeeRepository, DynamoDbPayerPayeeRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health"),
                builder => builder.UseMiddleware<UserContextMiddleware>());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                    });
            });
        }
    }
}