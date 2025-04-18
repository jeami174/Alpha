using Data.Context;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;
using Data.Interfaces;

namespace Data.Repositories;

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

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            var entities = await _dbSet.ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all {nameof(TEntity)}: {ex.Message}");
            return [];
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllWithDetailsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;
            if (includeExpression != null)
            {
                query = includeExpression(query);
            }
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving all {nameof(TEntity)} with details: {ex.Message}");
            return Enumerable.Empty<TEntity>();
        }
    }

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

    public virtual async Task<TEntity?> GetOneWithDetailsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> includeExpression, Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;
            if (includeExpression != null)
            {
                query = includeExpression(query);
            }
            var entity = await query.FirstOrDefaultAsync(predicate);
            return entity;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error retrieving {nameof(TEntity)} with details: {ex.Message}");
            return null;
        }
    }

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