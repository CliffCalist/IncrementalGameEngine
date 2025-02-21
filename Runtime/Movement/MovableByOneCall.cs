using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class MovableByOneCall : DisposableBase, IMovableByOneCall, IMovableByOneCallProvider, IMovableProvider, IDisposable
    {
        private IDisposable _dynamicTargetSubscription;


        public abstract ReactiveProperty<bool> IsPaused { get; }

        public abstract ReadOnlyReactiveProperty<bool> IsArrived { get; }
        public abstract ReadOnlyReactiveProperty<bool> IsMoving { get; }


        protected readonly ReactiveProperty<Vector3?> _targetPosition = new();
        public ReadOnlyReactiveProperty<Vector3?> TargetPosition => _targetPosition;

        protected readonly ReactivePropertyUpdater<Vector3> _selfPosition;
        public ReadOnlyReactiveProperty<Vector3> SelfPosition => _selfPosition;

        protected readonly ReactiveProperty<Vector3?> _lastCalculatedSelfPosition = new(null);
        public ReadOnlyReactiveProperty<Vector3?> LastCalculatedSelfPosition => _lastCalculatedSelfPosition;



        IMovableByOneCall IMovableByOneCallProvider.Movement => this;
        IMovable IMovableProvider.Movement => this;



        public MovableByOneCall(ReactivePropertyUpdater<Vector3> selfPosition)
        {
            _selfPosition = selfPosition ?? throw new ArgumentNullException(nameof(selfPosition));

            BuildPermanentDisposable(
                _selfPosition, _targetPosition, _lastCalculatedSelfPosition
            );
        }



        public void Wrap(Vector3 newPosition)
        {
            ThrowIfDisposed();

            _targetPosition.Value = null;
            _lastCalculatedSelfPosition.Value = newPosition;
        }



        public void SetTarget(Vector3? target)
        {
            ThrowIfDisposed();

            _dynamicTargetSubscription?.Dispose();
            _targetPosition.Value = target;
        }



        public void SetDynamicTarget(ReadOnlyReactiveProperty<Vector3> target, bool disposeIfArrived)
        {
            ThrowIfDisposed();

            if (target is null)
                throw new ArgumentNullException(nameof(target));


            _targetPosition.Value = target.CurrentValue;

            _dynamicTargetSubscription?.Dispose();
            _dynamicTargetSubscription = target
                .DistinctUntilChanged()
                .Subscribe(p => _targetPosition.Value = p);


            if (disposeIfArrived)
            {
                IsArrived
                    .Where(s => s)
                    .Take(1)
                    .Subscribe(_ => _dynamicTargetSubscription?.Dispose());
            }
        }



        protected override void DisposeProtected()
        {
            _dynamicTargetSubscription?.Dispose();
            base.DisposeProtected();
        }
    }
}