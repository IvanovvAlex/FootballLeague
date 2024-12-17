using FootballLeague.Common.Requests.Team;
using FootballLeague.Common.Responses.Team;
using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using FootballLeague.Domain.Interfaces;
using FootballLeague.Shared.Exceptions;

namespace FootballLeague.Domain.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;

        public TeamService(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamResponse> CreateAsync(CreateTeamRequest request)
        {
            if (await _teamRepository.IsTeamNameAlreadyInUse(request.Name))
            {
                throw new AppException("Team name already exists!").SetStatusCode(409);
            }

            Team newTeam = new Team
            {
                Name = request.Name
            };

            await _teamRepository.AddAsync(newTeam);

            return new TeamResponse
            {
                Id = newTeam.Id,
                Name = newTeam.Name,
            };
        }

        public async Task<TeamResponse?> GetByIdAsync(Guid id)
        {
            Team? team = await _teamRepository.GetByIdAsync(id);

            return team == null ? null : new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                Rank = team.Rank
            };

        }

        public async Task<IEnumerable<TeamResponse>> GetAsync()
        {
            IEnumerable<Team> teams = await _teamRepository.GetAllAsync();

            return teams.Select(team => new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                Rank = team.Rank
            });
        }

        public async Task<TeamResponse?> UpdateAsync(UpdateTeamRequest request)
        {
            Team? updatedTeam = new Team
            {
                Id = request.Id,
                Name = request.Name
            };

            updatedTeam = await _teamRepository.UpdateAsync(updatedTeam);

            if (updatedTeam == null)
            {
                return null;
            }

            return new TeamResponse
            {
                Id = updatedTeam.Id,
                Name = updatedTeam.Name,
                Rank = updatedTeam.Rank
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _teamRepository.IsTeamExists(id))
            {
                throw new AppException("Team does not exist!").SetStatusCode(404);
            }

            if (!await _teamRepository.DeleteAsync(id))
            {
                return false;
            }

            return true;
        }
    }
}