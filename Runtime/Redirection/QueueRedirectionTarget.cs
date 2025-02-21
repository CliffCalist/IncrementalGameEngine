using System;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class QueueRedirectionTarget<TEntityViewModel> : RedirectionTarget<TEntityViewModel>
        where TEntityViewModel : IMovableByOneCallProvider
    {
        private EntityQueue<TEntityViewModel> _queue;

        private readonly ObservableList<TEntityViewModel> _entities = new();
        public override IReadOnlyObservableList<TEntityViewModel> Entities => _entities;


        private readonly ReactiveProperty<bool> _hasFreeSeat = new();
        public override ReadOnlyReactiveProperty<bool> HasFreeSeat => _hasFreeSeat;

        private readonly ReactiveProperty<bool> _canGet = new();
        public ReadOnlyReactiveProperty<bool> CanGet => _canGet;

        private readonly ReactiveProperty<int> _seatsCount = new();
        public override ReadOnlyReactiveProperty<int> SeatsCount => _seatsCount;



        public QueueRedirectionTarget(EntityQueue<TEntityViewModel> queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));


            var queueAddSubscription = _queue.Entities.ObserveAdd()
                .Subscribe(arg => _entities.Add(arg.Value));

            var queueRemoveSubscription = _queue.Entities.ObserveRemove()
                .Subscribe(arg => _entities.Remove(arg.Value));


            var queueSlotsCountSubscription = _queue.SlotsCount
                .Subscribe(c => _seatsCount.Value = c);

            var queueCanEnqueueSubscription = _queue.CanEnqueue
                .Subscribe(s => _hasFreeSeat.Value = s);

            var queueCanDequeueSubscription = _queue.CanDequeue
                .Subscribe(s => _canGet.Value = s);


            BuildPermanentDisposable(
                queueAddSubscription, queueRemoveSubscription, queueCanEnqueueSubscription, queueCanDequeueSubscription,
                queueSlotsCountSubscription, _hasFreeSeat, _seatsCount
            );
        }


        protected override bool TryPlaceProtected(TEntityViewModel entity)
        {
            ThrowIfDisposed();

            return _queue.TryEnqueue(entity);
        }


        protected override bool TryRemoveEntityPeekProtected(out TEntityViewModel entity, bool isArrived)
        {
            ThrowIfDisposed();

            if (_queue.TryDequeuePeek(out var dequeuedEntity))
            {
                entity = dequeuedEntity;
                return true;
            }
            else
            {
                entity = default;
                return false;
            }
        }

        protected override bool TryRemoveEntityProtected(TEntityViewModel entity, bool isArrived)
        {
            ThrowIfDisposed();

            if (isArrived && !entity.Movement.IsArrived.CurrentValue)
                return false;

            return _queue.TryDequeue(entity);
        }


        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _entities.Clear();
            _queue = null;
        }
    }
}