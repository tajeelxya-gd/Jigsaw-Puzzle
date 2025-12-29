using System.Collections.Generic;

namespace UniTx.Runtime.Entity
{
    /// <summary>
    /// Service for retrieving entity instances by id or type.
    /// </summary>
    public interface IEntityService
    {
        TEntity Get<TEntity>(string id)
            where TEntity : IEntity;

        IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : IEntity;
    }
}