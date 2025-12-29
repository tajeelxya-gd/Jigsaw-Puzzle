using UniTx.Runtime.Content;

namespace UniTx.Runtime.Entity
{
    /// <summary>
    /// Describes data required to create or identify an entity.
    /// </summary>
    public interface IEntityData : IData
    {
        string Name { get; }

        IEntity CreateEntity();
    }
}