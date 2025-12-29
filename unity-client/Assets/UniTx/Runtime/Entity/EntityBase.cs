using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Serialisation;

namespace UniTx.Runtime.Entity
{
    public abstract class EntityBase<TData, TSavedData> : IEntity
         where TData : class, IEntityData
         where TSavedData : class, ISavedData, new()
    {
        protected ISerialisationService _serialisationService;
        protected IContentService _contentservice;

        public TData Data { get; private set; }
        public TSavedData SavedData { get; private set; }

        public string Id { get; private set; }

        protected EntityBase(string id) => Id = id;

        protected abstract void OnInject(IResolver resolver);

        protected abstract void OnInit();

        protected abstract void OnReset();

        public void Inject(IResolver resolver)
        {
            _serialisationService = resolver.Resolve<ISerialisationService>();
            _contentservice = resolver.Resolve<IContentService>();
            OnInject(resolver);
        }

        public void Initialise()
        {
            Data = _contentservice.GetData<TData>(Id);
            SavedData = _serialisationService.Load<TSavedData>(Id);
            OnInit();
        }

        public void Reset()
        {
            OnReset();
            Data = null;
            SavedData = null;
        }

        public void Save() => _serialisationService.Save(SavedData);
    }
}