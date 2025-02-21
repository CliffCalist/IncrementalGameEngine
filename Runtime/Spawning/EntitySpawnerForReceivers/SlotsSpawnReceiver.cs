using System.Collections.Generic;
using System.Linq;
using R3;

namespace WhiteArrow.Incremental
{
    public class SlotsSpawnReceiver<TViewModel> : DisposableBase, ISpawnReceiver<TViewModel>
    {
        private readonly List<Slot<TViewModel>> _slots = new();


        public ReadOnlyReactiveProperty<int> SlotsCount { get; }
        public ReadOnlyReactiveProperty<int> OccupiedSlotsCount { get; }

        private readonly Subject<Unit> _onSlotFreeded = new();
        public Observable<Unit> OnSlotFreeded => _onSlotFreeded;



        public SlotsSpawnReceiver(List<Slot<TViewModel>> slots)
        {
            _slots = slots ?? throw new System.ArgumentNullException(nameof(slots));


            SlotsCount = new ReactiveProperty<int>(_slots.Count);

            OccupiedSlotsCount = Observable.CombineLatest(_slots.Select(s => s.IsFree))
                .Select(states => states.Count(s => !s))
                .ToReadOnlyReactiveProperty();

            foreach (var slot in _slots)
                slot.IsFree.Where(s => s).Subscribe(_ => _onSlotFreeded.OnNext(Unit.Default));

            BuildPermanentDisposable(SlotsCount, OccupiedSlotsCount, _onSlotFreeded);
        }



        public bool TryReceiveSpawnedEntity(TViewModel item)
        {
            ThrowIfDisposed();

            if (SlotsCount.CurrentValue <= OccupiedSlotsCount.CurrentValue)
                return false;
            else
            {
                var slot = _slots.Find(s => s.IsFree.CurrentValue);
                slot.Item.Value = item;
                return true;
            }
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();

            foreach (var slot in _slots)
                slot.Dispose();
            _slots.Clear();
        }
    }
}