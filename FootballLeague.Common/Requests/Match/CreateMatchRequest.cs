using System.ComponentModel.DataAnnotations;

namespace FootballLeague.Common.Requests.Match
{
    public class CreateMatchRequest
    {
        [Required]
        public required Guid HomeTeamId { get; set; }

        [Required]
        public required Guid AwayTeamId { get; set; }

        [Required]
        public required int HomeTeamScore { get; set; }

        [Required]
        public required int AwayTeamScore { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}