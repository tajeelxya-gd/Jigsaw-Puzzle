namespace UniTx.Runtime.Entity
{
    public interface IEntityLoader
    {
        void LoadEntities();

        void UnloadEntities();
    }
}