using FootballLeague.Common.Responses.Team;

namespace FootballLeague.Common.Responses.Match
{
    public class MatchResponse
    {
        public Guid Id { get; set; }

        public Guid HomeTeamId { get; set; }

        public Guid AwayTeamId { get; set; }

        public int HomeTeamScore { get; set; }

        public int AwayTeamScore { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TeamResponse? HomeTeam { get; set; }

        public TeamResponse? AwayTeam { get; set; }
    }
}