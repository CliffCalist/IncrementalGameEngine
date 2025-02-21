using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class EntitiesLifecycle<TViewModel, TFactoryInput> : DisposableBase, IEntitiesLifecycle<TViewModel, TFactoryInput>
    {
        private readonly IEntityFactory<TViewModel, TFactoryInput> _factory;


        private readonly ObservableList<TViewModel> _viewModels = new();
        public IReadOnlyObservableList<TViewModel> ViewModels => _viewModels;

        private readonly Dictionary<TViewModel, IDisposable> _destroySubsctionsMap = new();

        public ReadOnlyReactiveProperty<int> Count { get; }



        public EntitiesLifecycle(IEntityFactory<TViewModel, TFactoryInput> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            Count = _viewModels.ObserveCountChanged()
                .ToReadOnlyReactiveProperty();



            var vieModelAddSubscription = _viewModels.ObserveAdd()
                .Subscribe(arg =>
                {
                    var viewModel = arg.Value;

                    if (viewModel is IDestroyable destroyableViewModel)
                    {
                        var subsription = destroyableViewModel.IsDestroyed
                            .Where(s => s)
                            .Subscribe(_ => _viewModels.Remove(viewModel));

                        _destroySubsctionsMap.Add(viewModel, subsription);
                    }
                });

            var vieModelRemoveSubscription = _viewModels.ObserveRemove()
                .Subscribe(arg =>
                {
                    var viewModel = arg.Value;

                    if (_destroySubsctionsMap.TryGetValue(viewModel, out var subscription))
                    {
                        _destroySubsctionsMap.Remove(viewModel);
                        subscription.Dispose();
                    }
                });



            BuildPermanentDisposable(vieModelAddSubscription, vieModelRemoveSubscription, Count);
        }



        public Observable<TViewModel> Create(TFactoryInput factoryInput)
        {
            ThrowIfDisposed();

            if (factoryInput is null)
                throw new ArgumentNullException(nameof(factoryInput));

            return _factory.Build(factoryInput)
                .Select(viewModel =>
                {
                    Add(viewModel);
                    return viewModel;
                });
        }

        public void Add(TViewModel viewModel)
        {
            ThrowIfDisposed();

            if (_viewModels.Contains(viewModel))
                throw new InvalidOperationException($"{nameof(TViewModel)} is already managed.");

            _viewModels.Add(viewModel);
        }



        public void Destroy(TViewModel viewModel)
        {
            ThrowIfDisposed();

            if (_viewModels.Contains(viewModel))
            {
                _viewModels.Remove(viewModel);

                if (viewModel is IDestroyable destroyableViewModel)
                    Destroying.Destr(destroyableViewModel);
                else if (viewModel is IDisposable disposableViewModel)
                    disposableViewModel.Dispose();
            }
        }

        public void DestroyLast()
        {
            ThrowIfDisposed();

            var last = _viewModels.LastOrDefault();
            if (!last.Equals(default))
                Destroy(last);
        }

        public void DestroyAll()
        {
            ThrowIfDisposed();

            while (_viewModels.Count > 0)
            {
                var viewModel = _viewModels.First();
                Destroy(viewModel);
            }
        }



        protected override void DisposeProtected()
        {
            DestroyAll();
        }
    }
}