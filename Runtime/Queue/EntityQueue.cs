using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class EntityQueue<T> : DisposableBase, IEntityQueue<T>
        where T : IMovableByOneCallProvider
    {
        private readonly ObservableList<T> _entities = new();
        public IReadOnlyObservableList<T> Entities => _entities;

        private readonly ObservableList<Slot<T>> _slots = new();


        public ReactiveProperty<bool> IsStrictMode { get; } = new(true);


        public ReadOnlyReactiveProperty<T> Peek { get; }
        public ReadOnlyReactiveProperty<bool> IsCurrentPeekArrived { get; }


        public ReadOnlyReactiveProperty<bool> CanEnqueue { get; }
        public ReadOnlyReactiveProperty<bool> CanDequeue { get; }


        public ReadOnlyReactiveProperty<int> SlotsCount { get; }
        public ReadOnlyReactiveProperty<int> EnqueuedCount { get; }
        public ReadOnlyReactiveProperty<int> FreeSlotsCount { get; }


        private readonly Subject<Unit> _onDequeued = new();
        public Observable<Unit> OnDequeued => _onDequeued;



        public EntityQueue(IEnumerable<MonoDataProvider<Vector3>> slotsPositionProviders)
        {
            if (slotsPositionProviders is null)
                throw new ArgumentNullException(nameof(slotsPositionProviders));


            Peek = _entities.ObserveChanged()
                .Select(_ =>
                {
                    if (_entities.Count > 0)
                        return _entities[0];
                    else return default;
                })
                .ToReadOnlyReactiveProperty();

            IsCurrentPeekArrived = Peek
                .Select(e =>
                {
                    if (e != null)
                        return e.Movement.IsArrived.AsObservable();
                    else return Observable.Return(false);
                })
                .Switch()
                .ToReadOnlyReactiveProperty();

            EnqueuedCount = _entities
                .ObserveCountChanged()
                .ToReadOnlyReactiveProperty(_entities.Count);

            SlotsCount = _slots.ObserveCountChanged()
                .ToReadOnlyReactiveProperty(_slots.Count);

            FreeSlotsCount = SlotsCount
                .CombineLatest(
                    EnqueuedCount,
                    (seatsCount, enqueuedCount) => (seatsCount, enqueuedCount))
                .Select(e => e.seatsCount - e.enqueuedCount)
                .ToReadOnlyReactiveProperty();


            CanEnqueue = SlotsCount
                .CombineLatest(
                    EnqueuedCount,
                    (seatsCount, enqueuedCount) => (seatsCount, enqueuedCount))
                .Select(e => e.seatsCount > e.enqueuedCount)
                .ToReadOnlyReactiveProperty();

            CanDequeue = IsStrictMode
                .CombineLatest(
                    EnqueuedCount,
                    IsCurrentPeekArrived,
                    (isStrictMode, count, isCurrentArrived) => (isStrictMode, count, isCurrentArrived))
                .Select(e =>
                {
                    if (e.isStrictMode)
                    {
                        if (e.count <= 0)
                            return false;
                        else return e.isCurrentArrived;
                    }
                    else return e.count > 0;
                })
                .ToReadOnlyReactiveProperty();



            var entitiesRemoveSubscription = _entities.ObserveRemove()
                .Where(_ => SlotsCount.CurrentValue > 1)
                .Subscribe(_ =>
                {
                    for (int i = 0; i < _slots.Count; i++)
                    {
                        if (i >= EnqueuedCount.CurrentValue)
                        {
                            _slots[i].Item.Value = default;
                            continue;
                        }

                        if (_slots[i].IsFree.CurrentValue || !object.Equals(_slots[i].Item.CurrentValue, _entities[i]))
                        {
                            _entities[i].Movement.SetTarget(_slots[i].PositionProvider.Value);
                            _slots[i].Item.Value = _entities[i];
                        }
                    }

                    _onDequeued.OnNext(Unit.Default);
                });


            foreach (var seatPoint in slotsPositionProviders)
                _slots.Add(new(seatPoint));



            BuildPermanentDisposable(
                entitiesRemoveSubscription, IsStrictMode, Peek, IsCurrentPeekArrived, CanEnqueue, CanDequeue, SlotsCount,
                EnqueuedCount, FreeSlotsCount, _onDequeued
            );
        }



        public bool TryEnqueue(T movementProvider)
        {
            ThrowIfDisposed();

            if (CanEnqueue.CurrentValue)
            {
                var slot = _slots.FirstOrDefault(s => s.IsFree.CurrentValue);
                if (slot == null)
                    return false;

                slot.Item.Value = movementProvider;
                movementProvider.Movement.SetTarget(slot.PositionProvider.Value);

                _entities.Add(movementProvider);
                return true;
            }
            else return false;
        }



        public bool TryDequeuePeek(out T item)
        {
            ThrowIfDisposed();

            if (CanDequeue.CurrentValue)
            {
                _slots[0].Item.Value = default;
                item = _entities[0];
                _entities.RemoveAt(0);
                return true;
            }
            else
            {
                item = default;
                return false;
            }
        }

        public bool TryDequeueAt(int index, out T item)
        {
            ThrowIfDisposed();

            if (index >= 0 && index < _entities.Count)
            {
                item = _entities[index];
                _entities.RemoveAt(index);
            }

            item = default;
            return false;
        }

        public bool TryDequeue(T entity)
        {
            ThrowIfDisposed();

            if (_entities.Count <= 0 || entity == null)
                return false;

            _entities.Remove(entity);
            return true;
        }



        public void UpdateSeatPositions()
        {
            ThrowIfDisposed();

            foreach (var slot in _slots)
            {
                if (!slot.IsFree.CurrentValue)
                    slot.Item.Value.Movement.SetTarget(slot.PositionProvider.Value);
            }
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();

            _entities.Clear();

            foreach (var slot in _slots)
                slot.Dispose();
            _slots.Clear();
        }
    }
}