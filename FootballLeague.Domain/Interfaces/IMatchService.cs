using FootballLeague.Common.Requests.Match;
using FootballLeague.Common.Responses.Match;

namespace FootballLeague.Domain.Interfaces
{
    public interface IMatchService
    {
        Task<MatchResponse?> CreateAsync(CreateMatchRequest request);

        Task<MatchResponse?> GetByIdAsync(Guid id);

        Task<IEnumerable<MatchResponse>?> GetAsync();

        Task<MatchResponse?> UpdateAsync(UpdateMatchRequest request);

        Task<bool> DeleteAsync(Guid id);
    }
}