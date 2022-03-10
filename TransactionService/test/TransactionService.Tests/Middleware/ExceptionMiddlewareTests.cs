using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Middleware;
using TransactionService.Repositories.Exceptions;
using Xunit;

namespace TransactionService.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task GivenRepositoryItemExistsExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var expectedMessage = "message 123";
        var middleware = new ExceptionMiddleware(_ =>
            throw new RepositoryItemExistsException(expectedMessage));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

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
    public async Task GivenUnknownExceptionThrown_ThenCorrectProblemDetailsReturned()
    {
        var middleware = new ExceptionMiddleware(_ =>
            throw new Exception());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.Invoke(context);

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