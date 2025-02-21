using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public abstract class RedirectionTarget<TEntityViewModel> : DisposableBase, IRedirectionTarget<TEntityViewModel>, IRedirectionTargetProvider<TEntityViewModel>
        where TEntityViewModel : IMovableByOneCallProvider
    {
        private readonly Dictionary<TEntityViewModel, IDisposable> _entitiyArrivedSubscritpions = new();

        public ReactiveProperty<bool> IsEnabled { get; } = new(true);

        public abstract IReadOnlyObservableList<TEntityViewModel> Entities { get; }

        public abstract ReadOnlyReactiveProperty<bool> HasFreeSeat { get; }
        public abstract ReadOnlyReactiveProperty<int> SeatsCount { get; }


        private readonly Subject<Unit> _onRedirected = new();
        public Observable<Unit> OnRedirected => _onRedirected;

        private readonly Subject<Unit> _onArrived = new();
        public Observable<Unit> OnArrived => _onArrived;



        IRedirectionTarget<TEntityViewModel> IRedirectionTargetProvider<TEntityViewModel>.RedirectionTarget => this;


        public RedirectionTarget()
        {
            BuildPermanentDisposable(IsEnabled, _onRedirected, _onArrived);
        }


        public bool TryPlace(TEntityViewModel provider)
        {
            ThrowIfDisposed();

            if (IsEnabled.CurrentValue && HasFreeSeat.CurrentValue)
            {
                var result = TryPlaceProtected(provider);
                if (result)
                {
                    _onRedirected.OnNext(Unit.Default);

                    var arrivedSubscription = provider.Movement.IsArrived
                        .Where(s => s)
                        .Subscribe(_ => _onArrived.OnNext(Unit.Default));
                    _entitiyArrivedSubscritpions.Add(provider, arrivedSubscription);
                }
                return result;
            }
            else return false;
        }

        protected abstract bool TryPlaceProtected(TEntityViewModel entity);



        public bool TryRemoveEntityPeek(out TEntityViewModel movementProvider, bool isArrived = true)
        {
            ThrowIfDisposed();

            var result = TryRemoveEntityPeekProtected(out movementProvider, isArrived);
            if (result)
                RemoveEntityArrivedSubscription(movementProvider);

            return result;
        }

        protected abstract bool TryRemoveEntityPeekProtected(out TEntityViewModel entity, bool isArrived);



        public bool TryRemoveEntity(TEntityViewModel movementProvider, bool isArrived = true)
        {
            ThrowIfDisposed();

            var result = TryRemoveEntityProtected(movementProvider, isArrived);
            if (result)
                RemoveEntityArrivedSubscription(movementProvider);

            return result;
        }

        protected abstract bool TryRemoveEntityProtected(TEntityViewModel entity, bool isArrived);



        private void RemoveEntityArrivedSubscription(TEntityViewModel movementProvider)
        {
            ThrowIfDisposed();

            if (_entitiyArrivedSubscritpions.TryGetValue(movementProvider, out var arrivedSubscription))
            {
                arrivedSubscription.Dispose();
                _entitiyArrivedSubscritpions.Remove(movementProvider);
            }
        }



        protected override void DisposeProtected()
        {
            foreach (var kvp in _entitiyArrivedSubscritpions)
                kvp.Value.Dispose();
            base.DisposeProtected();
        }
    }
}