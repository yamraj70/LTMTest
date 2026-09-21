using LTMPorjectTes.Interface;
using LTMPorjectTes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LTMPorjectTes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IService _storyService;

        public StoryController(IService storyService)
        {
            _storyService = storyService;
        }

        [HttpGet("GetStories")]
        public async Task<ActionResult<List<ResponseModels>>> GetStories([FromQuery] int value)
        {
            try
            {
                if (value <= 0)
                {
                    return BadRequest();
                }

                var stories = await _storyService.GetBestStoriesAsync(value);

                return Ok(stories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

