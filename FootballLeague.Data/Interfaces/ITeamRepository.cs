using FootballLeague.Data.Entities;

namespace FootballLeague.Data.Interfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        public Task<bool> IsTeamNameAlreadyInUse(string name);

        public Task<bool> IsTeamExists(Guid id);

    }
}