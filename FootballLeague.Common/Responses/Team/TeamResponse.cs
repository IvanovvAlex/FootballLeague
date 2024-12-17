using FootballLeague.Common.Responses.Match;

namespace FootballLeague.Common.Responses.Team
{
    public class TeamResponse
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public int Rank { get; set; }
    }
}