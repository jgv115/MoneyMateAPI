﻿using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.Domain;
using TransactionService.Middleware;
using TransactionService.Models;
using TransactionService.Profiles;
using TransactionService.Repositories;

namespace TransactionService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddScoped<CurrentUserContext>();
            services.AddScoped<ITransactionHelperService, TransactionHelperService>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<IPayerPayeeService, PayerPayeeService>();

            services.AddAutoMapper(typeof(TransactionProfile), typeof(CategoryProfile));

            var awsLocalModeEnvVariable = Environment.GetEnvironmentVariable("AWS_LOCAL_MODE");
            var awsLocalMode = !string.IsNullOrWhiteSpace(awsLocalModeEnvVariable) && bool.Parse(awsLocalModeEnvVariable);

            if (awsLocalMode)
            {
                var awsServiceUrl = Environment.GetEnvironmentVariable("AWS_SERVICE_URL");
                var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");

                services.AddSingleton<IAmazonDynamoDB>(_ =>
                {
                    var clientConfig = new AmazonDynamoDBConfig
                    {
                        ServiceURL = awsServiceUrl,
                        AuthenticationRegion = awsRegion
                    };
                    return new AmazonDynamoDBClient(clientConfig);
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
            services.AddSingleton<ICategoriesRepository, DynamoDbCategoriesRepository>();
            services.AddSingleton<IPayerPayeeRepository, DynamoDbPayerPayeeRepository>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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