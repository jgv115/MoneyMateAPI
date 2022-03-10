using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                int returnedStatusCode;
                ProblemDetails problemDetails;
                switch (ex)
                {
                    case RepositoryItemExistsException:
                        returnedStatusCode = (int) HttpStatusCode.Conflict;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Conflict found when trying to create repository item",
                            Detail = ex.Message
                        };
                        break;
                    
                    default:
                        returnedStatusCode = (int) HttpStatusCode.InternalServerError;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Internal Server Error",
                            Detail = "A problem occured"
                        };
                        break;
                }

                httpContext.Response.ContentType = "application/problem+json";
                httpContext.Response.StatusCode = returnedStatusCode;
                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}