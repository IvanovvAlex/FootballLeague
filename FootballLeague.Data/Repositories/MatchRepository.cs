using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.Data.Repositories
{
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private readonly AppDbContext _context;
        private readonly ITeamRepository _teamRepository;
        public MatchRepository(AppDbContext context, ITeamRepository teamRepository) : base(context)
        {
            _context = context;
            _teamRepository = teamRepository;
        }

        public override async ValueTask<Match?> AddAsync(Match entity)
        {
            entity.HomeTeam = _context.Teams.Find(entity.HomeTeamId);
            entity.AwayTeam = _context.Teams.Find(entity.AwayTeamId);
            if (entity.HomeTeam == null || entity.AwayTeam == null)
            {
                return null;
            }

            _context.Matches.Add(entity);
            await _context.SaveChangesAsync();

            await GiveRank(entity);

            return entity;
        }

        public override async Task<IEnumerable<Match>> GetAllAsync()
        {
            return await _context.Matches
                .Where(m => m.IsDeleted == false)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .ToListAsync();
        }

        public override async ValueTask<Match?> GetByIdAsync(Guid id)
        {
            return await _context.Matches
                .Where(m => m.IsDeleted == false)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public override async ValueTask<Match?> UpdateAsync(Match entity)
        {
            Match? match = _context.Matches.FirstOrDefault(m => m.Id == entity.Id);

            if (match == null)
            {
                return null;
            }

            await RemoveRank(match);

            match.StartTime = entity.StartTime;
            match.EndTime = entity.EndTime;
            match.HomeTeamScore = entity.HomeTeamScore;
            match.AwayTeamScore = entity.AwayTeamScore;
            match.HomeTeamId = entity.HomeTeamId;
            match.AwayTeamId = entity.AwayTeamId;
            match.ModifiedOn = DateTime.UtcNow;

            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            await GiveRank(match);

            return match;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            Match? match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted == false);
            if (match == null)
            {
                return false;
            }

            if (await RemoveRank(match))
            {
                match.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> GiveRank(Match match)
        {
            Team? homeTeam = _context.Teams.Find(match.HomeTeamId);
            Team? awayTeam = _context.Teams.Find(match.AwayTeamId);

            if (homeTeam == null || awayTeam == null)
            {
                return false;
            }

            if (match.StartTime < DateTime.Now && match.EndTime < DateTime.Now)
            {
                if (match.HomeTeamScore > match.AwayTeamScore)
                {
                    homeTeam.Rank += 3;
                }
                else if (match.HomeTeamScore < match.AwayTeamScore)
                {
                    awayTeam.Rank += 3;
                }
                else
                {
                    homeTeam.Rank += 1;
                    awayTeam.Rank += 1;
                }
            }

            await _teamRepository.UpdateAsync(homeTeam);
            await _teamRepository.UpdateAsync(awayTeam);

            return true;
        }

        private async Task<bool> RemoveRank(Match match)
        {
            Team? homeTeam = _context.Teams.Find(match.HomeTeamId);
            Team? awayTeam = _context.Teams.Find(match.AwayTeamId);

            if (homeTeam == null || awayTeam == null)
            {
                return false;
            }

            if (match.HomeTeamScore > match.AwayTeamScore)
            {
                homeTeam.Rank -= 3;
            }
            else if (match.HomeTeamScore < match.AwayTeamScore)
            {
                awayTeam.Rank -= 3;
            }
            else
            {
                homeTeam.Rank -= 1;
                awayTeam.Rank -= 1;
            }

            await _teamRepository.UpdateAsync(homeTeam);
            await _teamRepository.UpdateAsync(awayTeam);

            return true;
        }
    }
}