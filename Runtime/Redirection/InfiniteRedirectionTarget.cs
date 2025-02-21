using System;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class InfiniteRedirectionTarget<TEntityViewModel> : RedirectionTarget<TEntityViewModel>
        where TEntityViewModel : IMovableByOneCallProvider
    {
        private MonoDataProvider<Vector3> _seatPosition;

        private readonly ObservableList<TEntityViewModel> _entities = new();
        public override IReadOnlyObservableList<TEntityViewModel> Entities => _entities;


        public override ReadOnlyReactiveProperty<bool> HasFreeSeat { get; } = new ReactiveProperty<bool>(true);
        public override ReadOnlyReactiveProperty<int> SeatsCount { get; } = new ReactiveProperty<int>(int.MaxValue);


        public InfiniteRedirectionTarget(MonoDataProvider<Vector3> seatPosition)
        {
            _seatPosition = seatPosition ?? throw new ArgumentNullException(nameof(seatPosition));

            BuildPermanentDisposable(_seatPosition, HasFreeSeat, SeatsCount);
        }


        protected override bool TryPlaceProtected(TEntityViewModel movemnetProvider)
        {
            ThrowIfDisposed();

            movemnetProvider.Movement.SetTarget(_seatPosition.Value);
            _entities.Add(movemnetProvider);

            return true;
        }

        protected override bool TryRemoveEntityPeekProtected(out TEntityViewModel entity, bool isArrived)
        {
            ThrowIfDisposed();

            entity = _entities.FirstOrDefault(e => isArrived ? e.Movement.IsArrived.CurrentValue : true);
            if (entity != null)
            {
                _entities.Remove(entity);
                return true;
            }
            else return false;
        }

        protected override bool TryRemoveEntityProtected(TEntityViewModel entity, bool isArrived)
        {
            ThrowIfDisposed();

            entity = _entities.FirstOrDefault(e => object.Equals(e, entity) && (isArrived ? e.Movement.IsArrived.CurrentValue : true));
            if (entity != null)
            {
                _entities.Remove(entity);
                return true;
            }
            else return false;
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _entities.Clear();
        }
    }
}