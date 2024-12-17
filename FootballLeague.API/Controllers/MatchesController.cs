using FootballLeague.API.Helpers;
using FootballLeague.Common.Requests.Match;
using FootballLeague.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FootballLeague.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchesController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return await ControllerProcessor.ProcessAsync(() => _matchService.GetAsync(), this);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return await ControllerProcessor.ProcessAsync(() => _matchService.GetByIdAsync(id), this);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateMatchRequest request)
        {
            return await ControllerProcessor.ProcessAsync(() => _matchService.CreateAsync(request), this, true);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateMatchRequest request)
        {
            return await ControllerProcessor.ProcessAsync(() => _matchService.UpdateAsync(request), this, true);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            return await ControllerProcessor.ProcessAsync<object>(
                async () => await _matchService.DeleteAsync(id), this);
        }
    }
}