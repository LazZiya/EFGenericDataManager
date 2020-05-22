using LazZiya.EFGenericDataManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LazZiya.EFGenericDataManager
{
    /// <summary>
    /// Generic CRUD manager
    /// </summary>
    public class EFGenericDataManager<TContext> : IEFGenericDataManager where TContext : DbContext
    {
        private readonly TContext Context;

        /// <summary>
        /// Initialize a new instance of EFGenericDataManager
        /// </summary>
        /// <param name="context"></param>
        public EFGenericDataManager(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
        }

        /// <summary>
        /// Get entity from db. 
        /// Use for Update, Delete operations.
        /// Using this method will keep tracking the entity.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TKey">Entity key type</typeparam>
        /// <param name="id">Entity id</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T, TKey>(TKey id)
            where T : class
            where TKey : IEquatable<TKey>
        {
            return await Context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Get entity from db
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="searchExpression">search expression</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression)
            where T : class
        {
            return await Context.Set<T>().AsNoTracking().SingleOrDefaultAsync(searchExpression);
        }

        /// <summary>
        /// Add a new entity to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync<T>(T entity)
            where T : class
        {
            Context.Attach(entity).State = EntityState.Added;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update entity, it must be fetched with FindAsync method.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<T, TKey>(T entity)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>
        {
            // check if entity is being tracked
            var local = Context.Set<T>().Local.SingleOrDefault(x => x.ID.Equals(entity.ID));

            // if entity is tracked detach it from context
            if (local != null)
                Context.Entry<T>(local).State = EntityState.Detached;

            Context.Attach(entity).State = EntityState.Modified;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update entity, it must be fetched with FindAsync method.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<T>(T entity)
            where T : class
        {
            Context.Attach(entity).State = EntityState.Modified;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync<T>(T entity)
        {
            Context.Attach(entity).State = EntityState.Deleted;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Determine if an entity is duplicated by given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<bool> IsDuplicate<T>(Expression<Func<T, bool>> expression)
            where T : class
        {
            return await Context.Set<T>().AsNoTracking().CountAsync(expression) > 0;
        }

        /// <summary>
        /// Set an entity IsDefault value to true, and all rest entitities to false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SetAsDefault<T, TKey>(TKey id)
            where T : class, IHasId<TKey>, IDefault
            where TKey : IEquatable<TKey>
        {
            // Set previous default to false
            var prevDefault = await GetAsync<T>(x => x.IsDefault == true);
            if (prevDefault != null)
            {
                prevDefault.IsDefault = false;
                var noOldDefault = await UpdateAsync(prevDefault);

                if (!noOldDefault)
                    throw new Exception("A previous record with default value can't be set to false.");
            }

            // Set current entity to default
            var entity = await GetAsync<T, TKey>(id);
            if (entity != null)
            {
                entity.IsDefault = true;
                return await UpdateAsync(entity);
            }

            // If we get here, something went wrong
            throw new Exception("Something went wrong!");
        }

        /// <summary>
        /// Returns a list of an ordered entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="searchExpressions"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public async Task<(ICollection<T>, int)> ListAsync<T>(int start, int count, List<Expression<Func<T, bool>>> searchExpressions, List<OrderByExpression<T>> orderBy)
            where T : class
        {
            var totalRecords = await Context.Set<T>().AsNoTracking().CountAsync();
            if (totalRecords == 0)
                return (null, totalRecords);

            var query = await Context.Set<T>().AsNoTracking()
                .WhereList<T>(searchExpressions)
                .OrderByList(orderBy)
                .Skip((start - 1) * count).Take(count)
                .ToListAsync();

            return (query, totalRecords);
        }
    }
}
