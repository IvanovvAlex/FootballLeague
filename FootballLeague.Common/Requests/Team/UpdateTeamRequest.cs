using System.ComponentModel.DataAnnotations;

namespace FootballLeague.Common.Requests.Team
{
    public class UpdateTeamRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}