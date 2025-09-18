using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrepTimerAPIs.Dtos;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Services;

namespace PrepTimerAPIs.Controllers
{
    [Authorize]
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

        [HttpGet]
        [EnableCors("customPolicy")]
        [Route("GetItemDetailsById/{itemId}")]
        public async Task<IActionResult> Get(int itemId)
        {
            var itemDetails = await _service.GetItemByIdAsync(itemId);
            return Ok(itemDetails);
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
            dto.ItemId = id;

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

        [HttpPost("test-item-create")]
        public async Task<IActionResult> CreateTestItem([FromBody] CreateItemRequestDTO request)
        {
            await _service.CreateTestItem(request);
            return Ok(new { message = "Item added successfully." });
        }
    }
}
