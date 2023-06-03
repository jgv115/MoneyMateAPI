using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionService.Controllers.Exceptions;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Middleware;
using TransactionService.Repositories.Exceptions;
using Xunit;

namespace TransactionService.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task GivenRepositoryItemDoesNotExistExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var expectedMessage = "message 123";
        var middleware = new ExceptionMiddleware(_ =>
            throw new RepositoryItemDoesNotExist(expectedMessage));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        await middleware.Invoke(context, logger.Object);

        Assert.Equal(404, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBodyString = reader.ReadToEnd();

        var expectedResponseBody = new ProblemDetails
        {
            Status = 404,
            Title = "Could not find item",
            Detail = expectedMessage
        };
        Assert.Equal(JsonSerializer.Serialize(expectedResponseBody), responseBodyString);
    }

    [Fact]
    public async Task GivenRepositoryItemExistsExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var expectedMessage = "message 123";
        var middleware = new ExceptionMiddleware(_ =>
            throw new RepositoryItemExistsException(expectedMessage));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        await middleware.Invoke(context, logger.Object);

        Assert.Equal(409, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBodyString = reader.ReadToEnd();

        var expectedResponseBody = new ProblemDetails
        {
            Status = 409,
            Title = "Conflict found when trying to create repository item",
            Detail = expectedMessage
        };
        Assert.Equal(JsonSerializer.Serialize(expectedResponseBody), responseBodyString);
    }

    [Fact]
    public async Task GivenUpdateCategoryOperationExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var expectedMessage = "message 123";
        var middleware = new ExceptionMiddleware(_ =>
            throw new UpdateCategoryOperationException(expectedMessage));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        await middleware.Invoke(context, logger.Object);

        Assert.Equal(400, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBodyString = reader.ReadToEnd();

        var expectedResponseBody = new ProblemDetails
        {
            Status = 400,
            Title = "Bad update category request",
            Detail = expectedMessage
        };
        Assert.Equal(JsonSerializer.Serialize(expectedResponseBody), responseBodyString);
    }

    [Fact]
    public async Task GivenQueryParameterInvalidExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        const string expectedMessage = "invalid query parameter!";
        var middleware = new ExceptionMiddleware(_ => throw new QueryParameterInvalidException(expectedMessage));

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var mockLogger = new Mock<ILogger<ExceptionMiddleware>>();

        await middleware.Invoke(httpContext, mockLogger.Object);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBodyString = reader.ReadToEnd();

        var expectedResponseBody = new ProblemDetails
        {
            Status = 400,
            Title = "Invalid query parameter",
            Detail = expectedMessage
        };

        Assert.Equal(JsonSerializer.Serialize(expectedResponseBody), responseBodyString);
    }


    [Fact]
    public async Task GivenUnknownExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var middleware = new ExceptionMiddleware(_ =>
            throw new Exception());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var logger = new Mock<ILogger<ExceptionMiddleware>>();

        await middleware.Invoke(context, logger.Object);

        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBodyString = reader.ReadToEnd();

        var expectedResponseBody = new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "A problem occured"
        };
        Assert.Equal(JsonSerializer.Serialize(expectedResponseBody), responseBodyString);
    }
}