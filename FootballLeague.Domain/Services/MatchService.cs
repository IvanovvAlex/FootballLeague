using FootballLeague.Common.Requests.Match;
using FootballLeague.Common.Responses.Match;
using FootballLeague.Common.Responses.Team;
using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using FootballLeague.Domain.Interfaces;
using FootballLeague.Shared.Exceptions;

namespace FootballLeague.Domain.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;

        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public async Task<MatchResponse> CreateAsync(CreateMatchRequest request)
        {
            Match newMatch = new Match
            {
                HomeTeamId = request.HomeTeamId,
                AwayTeamId = request.AwayTeamId,
                HomeTeamScore = request.HomeTeamScore,
                AwayTeamScore = request.AwayTeamScore,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            Match? createdMatch = await _matchRepository.AddAsync(newMatch);

            if (createdMatch == null)
            {
                throw new AppException("One or both teams do not exist.").SetStatusCode(404);
            }

            return new MatchResponse
            {
                Id = newMatch.Id,
                HomeTeamId = newMatch.HomeTeamId,
                AwayTeamId = newMatch.AwayTeamId,
                HomeTeamScore = newMatch.HomeTeamScore,
                AwayTeamScore = newMatch.AwayTeamScore,
                StartTime = newMatch.StartTime,
                EndTime = newMatch.EndTime,
            };
        }

        public async Task<MatchResponse?> GetByIdAsync(Guid id)
        {
            Match? match = await _matchRepository.GetByIdAsync(id);

            return match == null ? null : new MatchResponse
            {
                Id = match.Id,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                StartTime = match.StartTime,
                EndTime = match.EndTime,
                HomeTeam = new TeamResponse
                {
                    Id = match.HomeTeam!.Id,
                    Name = match.HomeTeam.Name,
                    Rank = match.HomeTeam.Rank
                },
                AwayTeam = new TeamResponse
                {
                    Id = match.AwayTeam!.Id,
                    Name = match.AwayTeam.Name,
                    Rank = match.AwayTeam.Rank
                }
            };
        }

        public async Task<IEnumerable<MatchResponse>> GetAsync()
        {
            IEnumerable<Match> matches = await _matchRepository.GetAllAsync();

            return matches.Select(match => new MatchResponse
            {
                Id = match.Id,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamScore = match.HomeTeamScore,
                AwayTeamScore = match.AwayTeamScore,
                StartTime = match.StartTime,
                EndTime = match.EndTime,
                HomeTeam = new TeamResponse
                {
                    Id = match.HomeTeam!.Id,
                    Name = match.HomeTeam.Name,
                    Rank = match.HomeTeam.Rank
                },
                AwayTeam = new TeamResponse
                {
                    Id = match.AwayTeam!.Id,
                    Name = match.AwayTeam.Name,
                    Rank = match.AwayTeam.Rank
                }
            });
        }

        public async Task<MatchResponse> UpdateAsync(UpdateMatchRequest request)
        {
            Match updatedMatch = new Match
            {
                Id = request.Id,
                HomeTeamId = request.HomeTeamId,
                AwayTeamId = request.AwayTeamId,
                HomeTeamScore = request.HomeTeamScore,
                AwayTeamScore = request.AwayTeamScore,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            await _matchRepository.UpdateAsync(updatedMatch);

            return new MatchResponse
            {
                Id = updatedMatch.Id,
                HomeTeamId = updatedMatch.HomeTeamId,
                AwayTeamId = updatedMatch.AwayTeamId,
                HomeTeamScore = updatedMatch.HomeTeamScore,
                AwayTeamScore = updatedMatch.AwayTeamScore,
                StartTime = updatedMatch.StartTime,
                EndTime = updatedMatch.EndTime,
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _matchRepository.DeleteAsync(id))
            {
                return false;
            }

            return true;
        }
    }
}