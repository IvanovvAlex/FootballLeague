using FootballLeague.API.Helpers;
using FootballLeague.Common.Requests.Team;
using FootballLeague.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FootballLeague.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return await ControllerProcessor.ProcessAsync(() => _teamService.GetAsync(), this);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return await ControllerProcessor.ProcessAsync(() => _teamService.GetByIdAsync(id), this);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTeamRequest request)
        {
            return await ControllerProcessor.ProcessAsync(() => _teamService.CreateAsync(request), this, true);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateTeamRequest request)
        {
            return await ControllerProcessor.ProcessAsync(() => _teamService.UpdateAsync(request), this, true);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            return await ControllerProcessor.ProcessAsync<object>(
                async () => await _teamService.DeleteAsync(id), this);
        }
    }
}