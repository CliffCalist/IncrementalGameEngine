using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class ResourceHolderFactory<TResourceView, TResourceViewModel, TResourceDataAdapter, TResourceData, TInput>
           : EntityByDataFactory<ResourceHolderMediator<TResourceViewModel, TResourceDataAdapter, TResourceData>, HolderDataAdapter<TResourceDataAdapter, TResourceData>, TInput>
           where TResourceView : Component
           where TResourceViewModel : IResourceProvider, IMovableByOneCallProvider, IDestroyable
           where TResourceDataAdapter : DataAdapter
           where TResourceData : IResourceData
           where TInput : HolderFactoryInput<TResourceDataAdapter, TResourceData>
    {
        private readonly HolderFactory<TResourceViewModel, TResourceDataAdapter, TResourceData> _holderFactory;
        protected readonly EntityByDataFactory<TResourceViewModel, TResourceDataAdapter, ResourceHolderFactoryInput<TResourceDataAdapter>> _resourceFactory;



        public ResourceHolderFactory(
            HolderFactory<TResourceViewModel, TResourceDataAdapter, TResourceData> holderFactory,
            EntityByDataFactory<TResourceViewModel, TResourceDataAdapter, ResourceHolderFactoryInput<TResourceDataAdapter>> resourceFactory)
        {
            _holderFactory = holderFactory ?? throw new ArgumentNullException(nameof(holderFactory));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }



        public override Observable<ResourceHolderMediator<TResourceViewModel, TResourceDataAdapter, TResourceData>> Build(TInput input)
        {
            return _holderFactory.Build(input)
                .Select(holder => CreateHolderMediator(holder));
        }

        protected virtual ResourceHolderMediator<TResourceViewModel, TResourceDataAdapter, TResourceData> CreateHolderMediator(
            Holder<TResourceViewModel, TResourceDataAdapter, TResourceData> holder)
        {
            return new ResourceHolderMediator<TResourceViewModel, TResourceDataAdapter, TResourceData>(
                holder,
                _resourceFactory,
                CreateResourceData,
                CreateResourceDataAdapter,
                CreateResourceFactoryInput
            );
        }


        protected abstract TResourceData CreateResourceData(ResourceType resourceType, long ammount);
        protected abstract TResourceDataAdapter CreateResourceDataAdapter(TResourceData data);
        protected abstract ResourceHolderFactoryInput<TResourceDataAdapter> CreateResourceFactoryInput(TResourceDataAdapter adapter, Vector3? spawnPosition);
    }
}