using System.Linq.Expressions;

namespace Data.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    #region Transaction Management
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    #endregion

    #region CRUD Operations
    Task CreateAsync(TEntity entity);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllWithDetailsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression);
    Task<TEntity?> GetOneAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetOneWithDetailsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression, Expression<Func<TEntity, bool>> predicate);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task SaveToDatabaseAsync();
    #endregion
}
