﻿using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.EntityFramework {

    /// <summary>
    /// A read/write base repository class, backed by
    /// a DbSet, exposing basic CRUD methods, as well
    /// as methods exposed by QueryableRepo 
    /// </summary>
    /// <typeparam name="TEntity">The associated model class</typeparam>
    /// <typeparam name="TContext">The associated DbContextBase class</typeparam>
    public abstract class WriteableRepo<TEntity, TContext>
            where TEntity : class, IHasSysUser, new()
            where TContext : DbContext {


        public TContext Context { get; set; }
        public string SysUser { get; set; }

        public ScopeProperties ScopeProperties { get; set; }


        /// <summary>
        /// Constructs a new RepoBase object using the provided DbContext
        /// </summary>
        /// <param name="context">Entity Framework DbContext</param>
        public WriteableRepo(TContext context, ScopeProperties scopeProperties) { 
            Context = context;
            ScopeProperties = scopeProperties;
        }



    /// <summary>
    /// Retrieves the entity with the provided primary key values
    /// </summary>
    /// <param name="keyValues">primary key provided as key-value object array</param>
    /// <returns>Entity whose primary key matches the provided input</returns>
    public virtual TEntity GetById(params object[] keyValues) {
        return Context.Find<TEntity>(keyValues);
    }


    /// <summary>
    /// Asychronously retrieves the entity with the provided primary key values.
    /// </summary>
    /// <param name="keyValues">primary key provided as key-value object array</param>
    /// <returns>Entity whose primary key matches the provided input</returns>
    public virtual async Task<TEntity> GetByIdAsync(params object[] keyValues) {
        return await Context.FindAsync<TEntity>(keyValues);
    }


    public IQueryable<TEntity> Query {
        get {
            return Context.Set<TEntity>()
                .AsNoTracking();
        }
    }



        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns>true if an entity with the provided keys exists</returns>
        public bool Exists(params object[] keyValues) {
            var entity = Context.Find<TEntity>(keyValues);
            Context.Entry(entity).State = EntityState.Detached;
            var exists = (entity != null);
            return exists;
        }


        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(params object[] keyValues) {
            var entity = await Context.FindAsync<TEntity>(keyValues);
            Context.Entry(entity).State = EntityState.Detached;
            var exists = (entity != null);
            return exists;
        }






        /// <summary>
        /// Creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        public virtual TEntity Create(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            entity.SysUser = ScopeProperties.User;
            Context.Add(entity);
            Context.SaveChanges();
            return entity;
        }


        /// <summary>
        /// Asynchronously creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        public virtual async Task<TEntity> CreateAsync(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            entity.SysUser = ScopeProperties.User;
            Context.Update(entity);
            await Context.SaveChangesAsync();
            return entity;
        }


        /// <summary>
        /// Updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>
        public virtual TEntity Update(TEntity entity, params object[] keyValues) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            var existing = Context.Find<TEntity>(keyValues);

            entity.SysUser = ScopeProperties.User;
            Context.Update(entity);
            Context.SaveChanges();

            return entity;
        }


        /// <summary>
        /// Asynchronously updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, params object[] keyValues) {

            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            var existing = await Context.FindAsync<TEntity>(keyValues);

            entity.SysUser = ScopeProperties.User;
            Context.Update(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual void Delete(params object[] keyValues) {

            var existing = Context.Find<TEntity>(keyValues);
            if (existing == null)
                throw new MissingEntityException(
                    $"Cannot find {new TEntity().GetType().Name} object with key value = {PrintKeys(keyValues)}");


            Context.Remove(existing);
            Context.SaveChanges();

        }

        /// <summary>
        /// Asynchrously deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual async Task DeleteAsync(params object[] keyValues) {
            var existing = Context.Find<TEntity>(keyValues);
            if (existing == null)
                throw new MissingEntityException(
                    $"Cannot find {new TEntity().GetType().Name} object with key value = {PrintKeys(keyValues)}");

            Context.Remove(existing);
            await Context.SaveChangesAsync();
        }



        protected string PrintKeys(params object[] keyValues) {
            return "[" + string.Join(",", keyValues) + "]";
        }





    }


}
