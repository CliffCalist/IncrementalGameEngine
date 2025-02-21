using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ResourceHolderMediator<TResourceViewModel, TResourceDataAdapter, TResourceData> : DisposableBase
        where TResourceViewModel : IResourceProvider, IMovableByOneCallProvider, IDestroyable
        where TResourceDataAdapter : DataAdapter
        where TResourceData : IResourceData
    {
        private readonly EntityByDataFactory<TResourceViewModel, TResourceDataAdapter, ResourceHolderFactoryInput<TResourceDataAdapter>> _resourceFactory;
        private readonly Func<ResourceType, long, TResourceData> _createResourceData;
        private readonly Func<TResourceData, TResourceDataAdapter> _createRsourceDataAdapter;
        private readonly Func<TResourceDataAdapter, Vector3?, ResourceHolderFactoryInput<TResourceDataAdapter>> _createResourceFactoryInput;


        public Holder<TResourceViewModel, TResourceDataAdapter, TResourceData> Holder { get; }



        public ResourceHolderMediator(
            Holder<TResourceViewModel, TResourceDataAdapter, TResourceData> holder,
            EntityByDataFactory<TResourceViewModel, TResourceDataAdapter, ResourceHolderFactoryInput<TResourceDataAdapter>> resourceFactory,
            Func<ResourceType, long, TResourceData> createResourceData,
            Func<TResourceData, TResourceDataAdapter> createResourceDataAdapter,
            Func<TResourceDataAdapter, Vector3?, ResourceHolderFactoryInput<TResourceDataAdapter>> createResourceFactoryInput)
        {
            Holder = holder ?? throw new ArgumentNullException(nameof(holder));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _createResourceData = createResourceData ?? throw new ArgumentNullException(nameof(createResourceData));
            _createRsourceDataAdapter = createResourceDataAdapter ?? throw new ArgumentNullException(nameof(createResourceDataAdapter));
            _createResourceFactoryInput = createResourceFactoryInput ?? throw new ArgumentNullException(nameof(createResourceFactoryInput));

            BuildPermanentDisposable(Holder);
        }



        public Observable<bool> TryHold(ResourceType resourceType, long ammount, Vector3? spawnPoint = null)
        {
            ThrowIfDisposed();

            var data = _createResourceData(resourceType, ammount);
            var adapter = _createRsourceDataAdapter(data);
            var input = _createResourceFactoryInput(adapter, spawnPoint);

            return _resourceFactory.Build(input)
                .Select(resource =>
                {
                    using (InlineDataSender<TResourceDataAdapter, TResourceData> sender = new(adapter, data))
                    {
                        var dataRelay = new DataRelay<TResourceViewModel, TResourceDataAdapter, TResourceData>(
                            sender,
                            resource,
                            adapter
                        );

                        if (Holder.TryHold(dataRelay))
                            return true;
                        else
                        {
                            Destroying.Destr(resource);
                            return false;
                        }
                    }
                });
        }



#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (!_isDisposed)
                Holder?.OnDrawGizmos();
        }
#endif
    }
}