using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class QueueSpawnReceiver<TViewModel> : DisposableBase, ISpawnReceiver<TViewModel>
        where TViewModel : IMovableByOneCallProvider
    {
        private EntityQueue<TViewModel> _queue;

        public ReadOnlyReactiveProperty<int> SlotsCount { get; }
        public ReadOnlyReactiveProperty<int> OccupiedSlotsCount { get; }

        private readonly Subject<Unit> _onSlotFreeded = new();
        public Observable<Unit> OnSlotFreeded => _onSlotFreeded;



        public QueueSpawnReceiver(EntityQueue<TViewModel> queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));

            SlotsCount = _queue.SlotsCount.Select(s => s).ToReadOnlyReactiveProperty();
            OccupiedSlotsCount = _queue.EnqueuedCount.Select(s => s).ToReadOnlyReactiveProperty();
            var onDequeuedSubscription = _queue.OnDequeued.Subscribe(_ => _onSlotFreeded.OnNext(Unit.Default));

            BuildPermanentDisposable(SlotsCount, OccupiedSlotsCount, _onSlotFreeded, onDequeuedSubscription);
        }



        public bool TryReceiveSpawnedEntity(TViewModel viewModel)
        {
            ThrowIfDisposed();

            return _queue.TryEnqueue(viewModel);
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _queue = null;
        }
    }
}