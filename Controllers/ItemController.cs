using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Services;

namespace PrepTimerAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _service;

        public ItemController(IItemService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var companyId = 1;
            var categories = await _service.GetItemsAsync(companyId);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromForm] ItemDto dto, IFormFile? logo)
        {
            await _service.AddItemAsync(dto, logo);
            return Ok(new { message = "Item added successfully." });
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] ItemDto dto, IFormFile? logo)
        {
            if (id != dto.ItemId)
                return BadRequest("Item ID mismatch");

            var result = await _service.UpdateItemAsync(dto, logo);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var result = await _service.DeleteItemAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
