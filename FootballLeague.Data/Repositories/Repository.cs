using System.Reflection;
using FootballLeague.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FootballLeague.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public virtual async ValueTask<TEntity?> AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsQueryable();
            PropertyInfo? isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");

            if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
            {
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
            }

            return await query.ToListAsync();
        }

        public virtual async ValueTask<TEntity?> GetByIdAsync(Guid id)
        {
            TEntity? entity = await _context.Set<TEntity>().FindAsync(id);
            PropertyInfo? isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");

            if (entity != null && isDeletedProperty != null)
            {
                bool isDeleted = (bool)isDeletedProperty.GetValue(entity);
                if (isDeleted)
                {
                    return null;
                }
            }

            return entity;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            TEntity? entity = await _context.Set<TEntity>().FindAsync(id);
            PropertyInfo? propertyInfo = entity.GetType().GetProperty("IsDeleted");

            if (entity == null)
            {
                return false;
            }

            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(entity, true, null);
                _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public virtual async ValueTask<TEntity?> UpdateAsync(TEntity entity)
        {
            Guid entityId = (Guid)entity.GetType().GetProperty("Id").GetValue(entity);
            TEntity? currentEntity = await _context.Set<TEntity>().FindAsync(entityId);

            if (currentEntity == null)
            {
                return null;
            }

            PropertyInfo? propertyInfo = entity.GetType().GetProperty("ModifiedOn");

            if (propertyInfo != null && propertyInfo.PropertyType == typeof(DateTime))
            {
                propertyInfo.SetValue(entity, DateTime.UtcNow, null);
            }

            EntityEntry<TEntity>? entry = _context.Entry(currentEntity);
            entry.CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
    }
}