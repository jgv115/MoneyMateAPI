using System;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Tags;
using MoneyMateApi.Repositories;
using Moq;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.Tags;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _mockRepository = new();
    [Fact]
    public async Task CreateTag_WhenCalled_ReturnsGuid()
    {
        // Arrange
        var tagService = new TagService(_mockRepository.Object);

        var tagName = "tagname";
        var expectedTag = new Tag(Guid.NewGuid(), tagName);
        _mockRepository.Setup(repository => repository.CreateTag(tagName))
            .ReturnsAsync(() => expectedTag);
        
        // Act
        var result = await tagService.CreateTag(tagName);

        // Assert
        Assert.Equal(expectedTag, result);
        _mockRepository.VerifyAll();
    }
}