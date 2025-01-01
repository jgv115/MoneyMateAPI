using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers.Tags;
using MoneyMateApi.Controllers.Tags.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Tags;
using Moq;
using Xunit;

namespace MoneyMateApi.Tests.Controllers;

public class TagControllerTests
{
    private readonly Mock<ITagService> _mockTagService = new();

    [Fact]
    public async Task GivenInputCreateTagDto_WhenCreateTagInvoked_ThenCorrectTagReturnedWith200OK()
    {
        var controller = new TagsController(_mockTagService.Object);

        var tagName = "tagName";
        var createTagDto = new CreateTagDto
        {
            Name = tagName
        };

        var expectedTag = new Tag(Guid.NewGuid().ToString(), tagName);
        _mockTagService.Setup(service => service.CreateTag(tagName)).ReturnsAsync(expectedTag);

        var result = await controller.CreateTag(createTagDto);

        var test = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedTag, test.Value);
        
        _mockTagService.VerifyAll();
    }
}