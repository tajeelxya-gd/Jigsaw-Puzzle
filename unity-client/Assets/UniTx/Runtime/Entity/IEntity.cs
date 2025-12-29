using UniTx.Runtime.IoC;

namespace UniTx.Runtime.Entity
{
    /// <summary>
    /// Runtime entity contract with lifecycle and persistence support.
    /// </summary>
    public interface IEntity : IInjectable, IInitialisable, IResettable
    {
        string Id { get; }

        void Save();
    }
}