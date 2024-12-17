using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.Data.Repositories
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private readonly AppDbContext _context;
        public TeamRepository(AppDbContext context)
            : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Team>> GetAllAsync()
        {
            return await _context.Teams
                .Where(t => t.IsDeleted == false)
                .Include(t => t.HomeMatches)
                .Include(t => t.AwayMatches)
                .OrderByDescending(t => t.Rank)
                .ToListAsync();
        }

        public override async ValueTask<Team?> GetByIdAsync(Guid id)
        {
            return await _context.Teams
                .Where(t => t.IsDeleted == false)
                .Include(t => t.HomeMatches)
                .Include(t => t.AwayMatches)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<bool> IsTeamExists(Guid id)
        {
            return _context.Teams.AnyAsync(t => t.Id == id && t.IsDeleted == false);
        }

        public Task<bool> IsTeamNameAlreadyInUse(string name)
        {
            return _context.Teams.AnyAsync(t => t.Name == name && t.IsDeleted == false);
        }
    }
}