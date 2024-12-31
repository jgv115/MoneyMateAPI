using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyMateApi.Controllers.Exceptions;
using MoneyMateApi.Domain.Services.Categories.Exceptions;
using MoneyMateApi.Middleware.Exceptions;
using MoneyMateApi.Repositories.Exceptions;

namespace MoneyMateApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<ExceptionMiddleware> logger)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error has been caught by the exception middleware");
                int returnedStatusCode;
                ProblemDetails problemDetails;
                switch (ex)
                {
                    case RepositoryItemDoesNotExist:
                        returnedStatusCode = (int) HttpStatusCode.NotFound;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Could not find item",
                            Detail = ex.Message
                        };
                        break;

                    case RepositoryItemExistsException:
                        returnedStatusCode = (int) HttpStatusCode.Conflict;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Conflict found when trying to create repository item",
                            Detail = ex.Message
                        };
                        break;

                    case UpdateCategoryOperationException:
                        returnedStatusCode = (int) HttpStatusCode.BadRequest;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Bad update category request",
                            Detail = ex.Message
                        };
                        break;

                    case QueryParameterInvalidException:
                        returnedStatusCode = (int) HttpStatusCode.BadRequest;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Invalid query parameter",
                            Detail = ex.Message
                        };
                        break;

                    case InvalidProfileIdException:
                        returnedStatusCode = (int) HttpStatusCode.BadRequest;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Invalid profileId",
                            Detail = ex.Message
                        };
                        break;
                    
                    case ProfileIdForbiddenException:
                        returnedStatusCode = (int) HttpStatusCode.Forbidden;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "User does not have access to this profile",
                            Detail = ex.Message
                        };
                        break;

                    default:
                        returnedStatusCode = (int) HttpStatusCode.InternalServerError;
                        problemDetails = new ProblemDetails
                        {
                            Status = returnedStatusCode,
                            Title = "Internal Server Error",
                            Detail = "A problem occured",
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