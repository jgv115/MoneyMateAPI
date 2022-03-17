using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Constants;
using TransactionService.Domain.Services;
using TransactionService.Dtos;

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
        public async Task<IActionResult> Get(TransactionType? transactionType = null)
        {
            var categoryTree = await _categoriesService.GetAllCategories(transactionType);
            return Ok(categoryTree);
        }

        // POST api/categories
        [HttpPost]
        public async Task<IActionResult> Post(CreateCategoryDto createCategoryDto)
        {
            await _categoriesService.CreateCategory(createCategoryDto);
            return Ok();
        }
        
        // GET api/categories/{categoryName}
        [HttpGet("{categoryName}")]
        public async Task<IActionResult> GetSubcategories(string categoryName)
        {
            var subCategories = await _categoriesService.GetSubcategories(categoryName);
            return Ok(subCategories);
        }
    }
}