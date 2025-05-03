using Data.Context;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;
using Data.Interfaces;

namespace Data.Repositories;

/// <summary>
/// Abstract base repository providing common EF Core data access functionality:
/// - Transaction management (Begin, Commit, Rollback)
/// - Basic CRUD operations (Create, Read, Update, Delete)
/// - Existence checks and saving changes
/// - Flexible querying with filtering, sorting, includes, and projection
/// </summary>
public abstract class BaseRepository<TEntity>(DataContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly DataContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    private IDbContextTransaction? _transaction = null;

    #region Transaction Management

    public virtual async Task BeginTransactionAsync()
    {
        _transaction ??= await _context.Database.BeginTransactionAsync();
    }

    public virtual async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public virtual async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region CRUD

    public virtual async Task CreateAsync(TEntity entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating {nameof(TEntity)}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Returns all entities of type TEntity.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all {nameof(TEntity)}: {ex.Message}");
            return Array.Empty<TEntity>();
        }
    }

    /// <summary>
    /// Returns entities projected to TSelect, with optional filtering, sorting, and includes.
    /// </summary>
    public virtual async Task<RepositoryResult<IEnumerable<TSelect>>> GetAllAsync<TSelect>(
        Expression<Func<TEntity, TSelect>> selector,
        bool orderByDescending = false,
        Expression<Func<TEntity, object>>? sortBy = null,
        Expression<Func<TEntity, bool>>? where = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (where != null)
            query = query.Where(where);

        if (includes?.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (sortBy != null)
            query = orderByDescending
                ? query.OrderByDescending(sortBy)
                : query.OrderBy(sortBy);

        var projected = await query
            .Select(selector)
            .ToListAsync();

        return new RepositoryResult<IEnumerable<TSelect>>
        {
            Succeeded = true,
            StatusCode = 200,
            Result = projected
        };
    }

    /// <summary>
    /// Retrieves all entities including related navigation properties.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllWithDetailsAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;
            if (includeExpression != null)
                query = includeExpression(query);

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all {nameof(TEntity)} with details: {ex.Message}");
            return Enumerable.Empty<TEntity>();
        }
    }

    /// <summary>
    /// Retrieves a single entity matching the given predicate.
    /// </summary>
    public virtual async Task<TEntity?> GetOneAsync(Expression<Func<TEntity, bool>> expression)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving {nameof(TEntity)}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Retrieves a single entity matching the predicate, including related data.
    /// </summary>
    public virtual async Task<TEntity?> GetOneWithDetailsAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression,
        Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;
            if (includeExpression != null)
                query = includeExpression(query);

            return await query.FirstOrDefaultAsync(predicate);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving {nameof(TEntity)} with details: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Marks the entity as modified in the context.
    /// </summary>
    public virtual void Update(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating {nameof(TEntity)}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Marks the entity for deletion from the context.
    /// </summary>
    public virtual void Delete(TEntity entity)
    {
        try
        {
            _dbSet.Remove(entity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing {nameof(TEntity)}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks if any entity matches the given predicate.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            return await _dbSet.AnyAsync(predicate);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking existence of {nameof(TEntity)}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Commits all pending changes to the database.
    /// </summary>
    public virtual async Task SaveToDatabaseAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving changes to {nameof(TEntity)}: {ex.Message}");
            throw;
        }
    }

    #endregion
}