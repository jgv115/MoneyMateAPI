using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain;

namespace TransactionService.Controllers
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
        public async Task<IActionResult> Get()
        {
            var categoryTree = await _categoriesService.GetAllCategories();
            return Ok(categoryTree);
        }

        [HttpGet("{categoryName}")]
        public async Task<IActionResult> GetSubCategories(string categoryName)
        {
            var subCategories = await _categoriesService.GetSubCategories(categoryName);
            return Ok(subCategories);
        }
    }
}