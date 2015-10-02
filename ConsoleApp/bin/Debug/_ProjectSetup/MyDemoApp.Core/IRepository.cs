namespace MyDemoApp.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Performs the CRUD operations on database.
    /// </summary>
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// Adds the entity into database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <returns>Returns the entity.</returns>
        TEntity Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Adds the entities into database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The entities object.</param>
        /// <returns>Returns the list of entities.</returns>
        IEnumerable<TEntity> AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        /// <summary>
        /// Deletes the entity from database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <returns>Returns the entity.</returns>
        TEntity Delete<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Deletes the entities from database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The entities object.</param>
        /// <returns>Returns the list of entities.</returns>
        IEnumerable<TEntity> DeleteRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        /// <summary>
        /// Queries the database object.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>Returns the IQueryable object of entities.</returns>
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;

        /// <summary>
        /// Saves the database changes.
        /// </summary>
        /// <returns>Returns the no of records affected.</returns>
        int SaveChanges();

        /// <summary>
        /// Updates the entity into database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="columnNames">The columnNames object.</param>
        /// <returns>Returns the entity object.</returns>
        TEntity Update<TEntity>(TEntity entity, IEnumerable<string> columnNames = null) where TEntity : class;

        /// <summary>
        /// Updates the entity into database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The entities object.</param>
        /// <param name="columnNames">The columnNames object.</param>
        /// <returns>Returns the list of entities.</returns>
        IEnumerable<TEntity> UpdateRange<TEntity>(IEnumerable<TEntity> entities, IEnumerable<string> columnNames = null) where TEntity : class;
    }
}