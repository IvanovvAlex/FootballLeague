using FootballLeague.Common.Requests.Team;
using FootballLeague.Common.Responses.Team;

namespace FootballLeague.Domain.Interfaces
{
    public interface ITeamService
    {
        Task<TeamResponse?> CreateAsync(CreateTeamRequest request);

        Task<TeamResponse?> GetByIdAsync(Guid id);

        Task<IEnumerable<TeamResponse>?> GetAsync();

        Task<TeamResponse?> UpdateAsync(UpdateTeamRequest request);

        Task<bool> DeleteAsync(Guid id);
    }
}