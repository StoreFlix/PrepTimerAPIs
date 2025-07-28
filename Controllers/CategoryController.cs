using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Services;

namespace PrepTimerAPIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var companyId = 1;
            var categories = await _service.GetCategoriesAsync(companyId);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDto dto)
        {
            await _service.AddCategoryAsync(dto);
            return Ok(new { message = "Category added successfully." });
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Ptcategory updatedCategory)
        {
            if (id != updatedCategory.CategoryId)
                return BadRequest("Category ID mismatch");

            var result = await _service.UpdateCategoryAsync(updatedCategory);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _service.DeleteCategoryAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }

}
