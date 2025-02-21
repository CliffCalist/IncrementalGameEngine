using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ResourceHolderFactoryByDelegates<TResourceView, TResourceViewModel, TResourceDataAdapter, TResourceData, TInput>
        : ResourceHolderFactory<TResourceView, TResourceViewModel, TResourceDataAdapter, TResourceData, TInput>
        where TResourceView : Component
        where TResourceViewModel : IResourceProvider, IMovableByOneCallProvider, IDestroyable
        where TResourceDataAdapter : DataAdapter
        where TResourceData : IResourceData
        where TInput : HolderFactoryInput<TResourceDataAdapter, TResourceData>
    {
        private readonly Func<ResourceType, long, TResourceData> _createResourceData;
        private readonly Func<TResourceData, TResourceDataAdapter> _createRsourceDataAdapter;
        private readonly Func<TResourceDataAdapter, Vector3?, ResourceHolderFactoryInput<TResourceDataAdapter>> _createResourceFactoryInput;



        public ResourceHolderFactoryByDelegates(
            HolderFactory<TResourceViewModel, TResourceDataAdapter, TResourceData> holderFactory,
            EntityByDataFactory<TResourceViewModel, TResourceDataAdapter, ResourceHolderFactoryInput<TResourceDataAdapter>> resourceFactory,
            Func<ResourceType, long, TResourceData> createResourceData,
            Func<TResourceData, TResourceDataAdapter> createResourceDataAdapter,
            Func<TResourceDataAdapter, Vector3?, ResourceHolderFactoryInput<TResourceDataAdapter>> createResourceFactoryInput)
            : base(holderFactory, resourceFactory)
        {
            _createResourceData = createResourceData ?? throw new ArgumentNullException(nameof(createResourceData));
            _createRsourceDataAdapter = createResourceDataAdapter ?? throw new ArgumentNullException(nameof(createResourceDataAdapter));
            _createResourceFactoryInput = createResourceFactoryInput ?? throw new ArgumentNullException(nameof(createResourceFactoryInput));
        }


        protected override TResourceData CreateResourceData(ResourceType resourceType, long ammount)
        {
            return _createResourceData(resourceType, ammount);
        }

        protected override TResourceDataAdapter CreateResourceDataAdapter(TResourceData data)
        {
            return _createRsourceDataAdapter(data);
        }

        protected override ResourceHolderFactoryInput<TResourceDataAdapter> CreateResourceFactoryInput(TResourceDataAdapter adapter, Vector3? spawnPosition)
        {
            return _createResourceFactoryInput(adapter, spawnPosition);
        }
    }
}