using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers.Tags.Dtos;
using MoneyMateApi.Domain.Tags;

namespace MoneyMateApi.Controllers.Tags;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag(CreateTagDto createTagDto)
    {
        var createdTag = await _tagService.CreateTag(createTagDto.Name);
        return new OkObjectResult(createdTag);
    }
}