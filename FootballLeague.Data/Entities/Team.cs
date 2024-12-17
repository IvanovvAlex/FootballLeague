namespace FootballLeague.Data.Entities
{
    public class Team : GenericEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string? Name { get; set; }

        public int Rank { get; set; }

        public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    }
}