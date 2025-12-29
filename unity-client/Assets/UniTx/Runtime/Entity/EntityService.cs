using System.Collections.Generic;
using System.Linq;
using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;

namespace UniTx.Runtime.Entity
{
    public sealed class EntityService : IEntityService, IEntityLoader
    {
        private readonly IDictionary<string, IEntity> _registry;
        private readonly IContentService _contentService;
        private readonly IResolver _resolver;

        public EntityService()
        {
            _registry = new Dictionary<string, IEntity>();
            var resolver = UniStatics.Resolver;
            _contentService = resolver.Resolve<IContentService>();
            _resolver = resolver;
        }

        public void LoadEntities()
        {
            var data = _contentService.GetAllData<IEntityData>();

            foreach (var datum in data)
            {
                var entity = datum.CreateEntity();
                entity.Inject(_resolver);
                entity.Initialise();
                _registry[entity.Id] = entity;
            }
        }

        public void UnloadEntities()
        {
            foreach (var entity in _registry.Values)
            {
                entity.Reset();
            }

            _registry.Clear();
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : IEntity
        {
            if (_registry.TryGetValue(id, out var asset) && asset is TEntity typedAsset)
            {
                return typedAsset;
            }

            throw new KeyNotFoundException($"Entity with Id '{id}' not found.");
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : IEntity
            => _registry.Values.OfType<TEntity>();
    }
}