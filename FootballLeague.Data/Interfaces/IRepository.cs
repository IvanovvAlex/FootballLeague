namespace FootballLeague.Data.Interfaces
{
    public interface IRepository<TEntity>
    where TEntity : class
    {
        ValueTask<TEntity?> GetByIdAsync(Guid id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        ValueTask<TEntity?> AddAsync(TEntity entity);

        Task<bool> DeleteAsync(Guid id);

        ValueTask<TEntity?> UpdateAsync(TEntity entity);
    }
}