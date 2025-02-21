using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class FiniteRedirectionTarget<TEntityViewModel> : RedirectionTarget<TEntityViewModel>
        where TEntityViewModel : IMovableByOneCallProvider
    {
        private readonly ObservableList<TEntityViewModel> _entities = new();
        public override IReadOnlyObservableList<TEntityViewModel> Entities => _entities;

        private readonly Dictionary<Vector3, TEntityViewModel> _seatsMap = new();


        public override ReadOnlyReactiveProperty<int> SeatsCount { get; }
        public override ReadOnlyReactiveProperty<bool> HasFreeSeat { get; }



        #region Constructors
        public FiniteRedirectionTarget(bool isEnabled, ICollection<Vector3> seats)
        {
            if (seats is null)
                throw new ArgumentNullException(nameof(seats));

            // TODO: Has be changed when has been aded oportunity adding and removing seats by methods
            foreach (var seat in seats)
                _seatsMap.Add(seat, default);
            SeatsCount = new ReactiveProperty<int>(_seatsMap.Count);

            IsEnabled.Value = isEnabled;


            HasFreeSeat = _entities.ObserveCountChanged()
                .CombineLatest(
                    SeatsCount,
                    (entitiesCount, seatsCount) => entitiesCount < seatsCount
                )
                .ToReadOnlyReactiveProperty(_entities.Count < SeatsCount.CurrentValue);


            BuildPermanentDisposable(HasFreeSeat);
        }



        public FiniteRedirectionTarget(bool isEnabled, Vector3 seat)
        {
            // TODO: Has be changed when has been aded oportunity adding and removing seats by methods
            _seatsMap.Add(seat, default);
            SeatsCount = new ReactiveProperty<int>(_seatsMap.Count);

            IsEnabled.Value = isEnabled;


            HasFreeSeat = _entities.ObserveCountChanged()
                .CombineLatest(
                    SeatsCount,
                    (entitiesCount, seatsCount) => entitiesCount < seatsCount
                )
                .ToReadOnlyReactiveProperty(_entities.Count < SeatsCount.CurrentValue);
        }
        #endregion



        #region Place/Remove
        protected override bool TryPlaceProtected(TEntityViewModel movementProvider)
        {
            ThrowIfDisposed();

            var freeSeat = FindFirstFreeSeat();
            if (!freeSeat.HasValue)
                throw new Exception($"Entity has been redirected, but target don't have free seat.");

            movementProvider.Movement.SetTarget(freeSeat.Value);
            _seatsMap[freeSeat.Value] = movementProvider;

            _entities.Add(movementProvider);
            return true;
        }

        protected override bool TryRemoveEntityPeekProtected(out TEntityViewModel entity, bool arrived)
        {
            ThrowIfDisposed();

            var lastNonFreeSeat = FindLastNonFreeSeat();
            if (lastNonFreeSeat == null)
            {
                entity = default;
                return false;
            }

            entity = _seatsMap[lastNonFreeSeat.Value];
            _seatsMap[lastNonFreeSeat.Value] = default;
            _entities.Remove(entity);
            return true;
        }

        protected override bool TryRemoveEntityProtected(TEntityViewModel entity, bool isArrived)
        {
            ThrowIfDisposed();

            var seat = FindSeatByEntity(entity);
            if (!seat.HasValue)
                return false;

            _seatsMap[seat.Value] = default;
            _entities.Remove(entity);
            return true;
        }
        #endregion



        #region Seats
        private Vector3? FindFirstFreeSeat()
        {
            ThrowIfDisposed();

            // TODO: can be optimized
            return _seatsMap.First(kvp => kvp.Value == null).Key;
        }

        private Vector3? FindLastNonFreeSeat()
        {
            ThrowIfDisposed();

            // TODO: can be optimized
            return _seatsMap.Last(kvp => kvp.Value != null).Key;
        }

        private Vector3? FindSeatByEntity(TEntityViewModel entity)
        {
            ThrowIfDisposed();

            return _seatsMap.First(kvp => object.Equals(kvp.Value, entity)).Key;
        }
        #endregion



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _entities.Clear();
        }
    }
}