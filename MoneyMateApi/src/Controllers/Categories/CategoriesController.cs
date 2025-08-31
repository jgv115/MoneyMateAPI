using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Categories;

namespace MoneyMateApi.Controllers.Categories
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService ?? throw new ArgumentNullException(nameof(categoriesService));
        }

        // GET api/categories
        [HttpGet]
        public async Task<IActionResult> Get(TransactionType? transactionType = null)
        {
            var categoryTree = await _categoriesService.GetAllCategories(transactionType);
            return Ok(categoryTree);
        }

        // GET api/categories/{categoryName}
        [HttpGet("{categoryName}")]
        public async Task<IActionResult> GetSubcategories(string categoryName)
        {
            var subCategories = await _categoriesService.GetSubcategories(categoryName);
            return Ok(subCategories);
        }

        // POST api/categories
        [HttpPost]
        public async Task<IActionResult> Post(CategoryDto categoryDto)
        {
            await _categoriesService.CreateCategory(categoryDto);
            return Ok();
        }

        [HttpDelete("{categoryName}")]
        public async Task<IActionResult> Delete(string categoryName)
        {
            await _categoriesService.DeleteCategory(categoryName);
            return Ok();
        }

        // PATCH api/categories/{categoryName}
        [HttpPatch("{categoryName}")]
        public async Task<IActionResult> Patch(string categoryName,
            [FromBody] JsonPatchDocument<CategoryDto> jsonPatchDocument)
        {
            await _categoriesService.UpdateCategory(categoryName, jsonPatchDocument);
            return Ok();
        }
    }
}