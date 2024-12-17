using System.ComponentModel.DataAnnotations;

namespace FootballLeague.Common.Requests.Team
{
    public class CreateTeamRequest
    {
        [Required]
        public required string Name { get; set; }
    }
}