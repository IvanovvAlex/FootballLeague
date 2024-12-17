namespace FootballLeague.Data.Entities
{
    public class Match : GenericEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid HomeTeamId { get; set; }

        public Guid AwayTeamId { get; set; }

        public int HomeTeamScore { get; set; }

        public int AwayTeamScore { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Team? HomeTeam { get; set; }

        public Team? AwayTeam { get; set; }
    }
}