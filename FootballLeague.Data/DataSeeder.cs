
using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace FootballLeague.Data
{
    public class DataSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public DataSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SeedAsync()
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ITeamRepository teamRepository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
                IMatchRepository matchRepository = scope.ServiceProvider.GetRequiredService<IMatchRepository>();

                IEnumerable<Entities.Team> teams = await teamRepository.GetAllAsync();
                IEnumerable<Entities.Match> matches = await matchRepository.GetAllAsync();

                if (!teams.Any() && !matches.Any())
                {
                    await SeedTeamsAsync(teamRepository);
                    await SeedMatchesAsync(matchRepository, teamRepository);
                }
            }
        }

        private async Task SeedTeamsAsync(ITeamRepository teamRepository)
        {
            List<Team> initialTeams = new List<Team>
        {
            new Team { Name = "Dunav" },
            new Team { Name = "Ludogorets" },
            new Team { Name = "CSKA" },
            new Team { Name = "Levski" },
        };

            foreach (Team team in initialTeams)
            {
                await teamRepository.AddAsync(team);
            }
        }

        private async Task SeedMatchesAsync(IMatchRepository matchRepository, ITeamRepository teamRepository)
        {
            IEnumerable<Team> teams = await teamRepository.GetAllAsync();
            List<Team> teamList = teams.ToList();

            // Ensure there are exactly 4 teams
            if (teamList.Count < 4)
            {
                throw new InvalidOperationException("Not enough teams available to seed matches.");
            }

            Guid dunavId = teamList.First(t => t.Name == "Dunav").Id;
            Guid ludogoretsId = teamList.First(t => t.Name == "Ludogorets").Id;
            Guid cskaId = teamList.First(t => t.Name == "CSKA").Id;
            Guid levskiId = teamList.First(t => t.Name == "Levski").Id;

            DateTime startDate = DateTime.UtcNow;

            List<Match> initialMatches = new List<Match>
    {
        // Matches for dunavId
        new Match { HomeTeamId = dunavId, AwayTeamId = ludogoretsId, HomeTeamScore = 1, AwayTeamScore = 1, StartTime = startDate, EndTime = startDate.AddHours(2) },
        new Match { HomeTeamId = dunavId, AwayTeamId = cskaId, HomeTeamScore = 2, AwayTeamScore = 1, StartTime = startDate.AddDays(1), EndTime = startDate.AddDays(1).AddHours(2) },
        new Match { HomeTeamId = levskiId, AwayTeamId = dunavId, HomeTeamScore = 3, AwayTeamScore = 0, StartTime = startDate.AddDays(2), EndTime = startDate.AddDays(2).AddHours(2) },

        // Matches for ludogoretsId
        new Match { HomeTeamId = ludogoretsId, AwayTeamId = cskaId, HomeTeamScore = 3, AwayTeamScore = 2, StartTime = startDate.AddDays(3), EndTime = startDate.AddDays(3).AddHours(2) },
        new Match { HomeTeamId = ludogoretsId, AwayTeamId = levskiId, HomeTeamScore = 1, AwayTeamScore = 1, StartTime = startDate.AddDays(4), EndTime = startDate.AddDays(4).AddHours(2) },
        new Match { HomeTeamId = dunavId, AwayTeamId = ludogoretsId, HomeTeamScore = 2, AwayTeamScore = 0, StartTime = startDate.AddDays(5), EndTime = startDate.AddDays(5).AddHours(2) },

        // Matches for cskaId
        new Match { HomeTeamId = cskaId, AwayTeamId = levskiId, HomeTeamScore = 1, AwayTeamScore = 1, StartTime = startDate.AddDays(6), EndTime = startDate.AddDays(6).AddHours(2) },
        new Match { HomeTeamId = cskaId, AwayTeamId = dunavId, HomeTeamScore = 1, AwayTeamScore = 2, StartTime = startDate.AddDays(7), EndTime = startDate.AddDays(7).AddHours(2) },
        new Match { HomeTeamId = ludogoretsId, AwayTeamId = cskaId, HomeTeamScore = 2, AwayTeamScore = 3, StartTime = startDate.AddDays(8), EndTime = startDate.AddDays(8).AddHours(2) },

        // Matches for levskiId
        new Match { HomeTeamId = levskiId, AwayTeamId = dunavId, HomeTeamScore = 3, AwayTeamScore = 0, StartTime = startDate.AddDays(9), EndTime = startDate.AddDays(9).AddHours(2) },
        new Match { HomeTeamId = levskiId, AwayTeamId = ludogoretsId, HomeTeamScore = 1, AwayTeamScore = 1, StartTime = startDate.AddDays(10), EndTime = startDate.AddDays(10).AddHours(2) },
        new Match { HomeTeamId = cskaId, AwayTeamId = levskiId, HomeTeamScore = 2, AwayTeamScore = 1, StartTime = startDate.AddDays(11), EndTime = startDate.AddDays(11).AddHours(2) },
    };

            foreach (Match match in initialMatches)
            {
                await matchRepository.AddAsync(match);
            }
        }
    }
}