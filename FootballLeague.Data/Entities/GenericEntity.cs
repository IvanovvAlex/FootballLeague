namespace FootballLeague.Data.Entities
{
    public class GenericEntity
    {
        public DateTime CreatedOn { get; init; } = DateTime.UtcNow;

        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
    }
}