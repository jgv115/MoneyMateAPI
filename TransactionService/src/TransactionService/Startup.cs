using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransactionService.Domain;
using TransactionService.Middleware;
using TransactionService.Models;
using TransactionService.Repositories;
using TransactionService.Settings;

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
            var auth0Settings = Configuration.GetSection(Auth0Settings.Key);
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

            // var dynamoDbConfig = Configuration.GetSection("DynamoDb");
            // var dynamoDbLocalMode = dynamoDbConfig.GetValue<bool>("LocalMode");
            //
            // if (dynamoDbLocalMode)
            // {
            //     services.AddSingleton<IAmazonDynamoDB>(provider =>
            //     {
            //         var clientConfig = new AmazonDynamoDBConfig()
            //         {
            //             ServiceURL = dynamoDbConfig.GetValue<string>("ServiceUrl")
            //         };
            //         return new AmazonDynamoDBClient(clientConfig);
            //     });
            // }
            // else
            // {
            //     services.AddAWSService<IAmazonDynamoDB>();
            // }
            services.AddSingleton<ITransactionRepository, MockTransactionRepository>();

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