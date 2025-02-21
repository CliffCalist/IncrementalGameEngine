using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class NavMeshMovement : MovableByOneCall, INavMeshMovementProvider
    {
        private readonly NavMeshMovementDataAdapter _data;
        private IDisposable _updateSubscription;


        public override ReactiveProperty<bool> IsPaused => _data.IsPaused;

        private readonly ReactiveProperty<bool> _isMoving = new(false);
        public override ReadOnlyReactiveProperty<bool> IsMoving => _isMoving;

        private readonly ReactiveProperty<bool> _isArrived = new(true);
        public override ReadOnlyReactiveProperty<bool> IsArrived => _isArrived;


        public ReactiveProperty<float> Speed => _data.Speed;
        public ReactiveProperty<float> Acceleration => _data.Acceleration;
        public ReactiveProperty<float> RotationSpeed => _data.RotationSpeed;



        NavMeshMovement INavMeshMovementProvider.NavMeshMovement => this;



        public NavMeshMovement(
            NavMeshMovementDataAdapter dataAdapter,
            ReactivePropertyUpdater<Vector3> selfPosition)
            : base(selfPosition)
        {
            _data = dataAdapter ?? throw new ArgumentNullException(nameof(dataAdapter));


            Update();

            var targetPosSubscription = _targetPosition
                .Subscribe(p =>
                {
                    _updateSubscription?.Dispose();
                    if (p.HasValue)
                    {
                        Update();
                        _updateSubscription = Observable.EveryUpdate(UnityFrameProvider.PostFixedUpdate)
                            .Subscribe(_ => Update());
                    }
                    else Update();
                });


            BuildPermanentDisposable(targetPosSubscription, _data, _isMoving, _isArrived);
        }


        public void Update()
        {
            ThrowIfDisposed();

            _isMoving.Value = _targetPosition.Value.HasValue ? !IsPositionEquals(_targetPosition.Value.Value, _selfPosition.Property.CurrentValue) : false;
            _isArrived.Value = _targetPosition.Value.HasValue ? IsPositionEquals(_targetPosition.Value.Value, _selfPosition.Property.CurrentValue) : true;
        }

        private bool IsPositionEquals(Vector3 first, Vector3 second)
        {
            ThrowIfDisposed();

            return first.x == second.x && first.z == second.z;
        }



        protected override void DisposeProtected()
        {
            _updateSubscription?.Dispose();
            base.DisposeProtected();
        }
    }
}