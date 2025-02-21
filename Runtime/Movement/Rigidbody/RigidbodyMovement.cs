using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class RigidbodyMovement : DisposableBase, IMovable, IMovableProvider, IDisposable
    {
        private readonly RigidbodyMovementDataAdapter _data;


        public ReadOnlyReactiveProperty<float> NonMoovingThreshold => _data.NonMoovingThreshold;
        public ReadOnlyReactiveProperty<bool> IsMoving { get; }


        public ReadOnlyReactiveProperty<float> MaxSpeed => _data.MaxSpeed;
        public ReadOnlyReactiveProperty<float> Speed => _data.AccelerationSpeed;
        public readonly ReactiveProperty<Vector3?> Direction = new();


        protected readonly ReactivePropertyUpdater<Vector3> _selfPositionUpdater;
        public ReadOnlyReactiveProperty<Vector3> SelfPosition => _selfPositionUpdater.Property;

        private readonly ReactivePropertyUpdater<Vector3> _velocityUpdater;
        public ReadOnlyReactiveProperty<Vector3> Velocity => _velocityUpdater.Property;


        private readonly ReactiveProperty<Vector3> _lastCalculatedVelocity = new();
        public ReadOnlyReactiveProperty<Vector3> LastCalculatedVelocity => _lastCalculatedVelocity;

        private readonly ReactiveProperty<Vector3> _lastCalculatedForce = new();
        public ReadOnlyReactiveProperty<Vector3> LastCalculatedForce => _lastCalculatedForce;

        private readonly ReactiveProperty<Vector3> _lastCalculatedPosition = new();
        public ReadOnlyReactiveProperty<Vector3> LastCalculatedPosition => _lastCalculatedPosition;



        IMovable IMovableProvider.Movement => this;


        public RigidbodyMovement(
            RigidbodyMovementDataAdapter data,
            MonoDataProvider<Vector3> velocityProvider,
            MonoDataProvider<Vector3> selfPositionProvider)
        {
            if (velocityProvider is null)
                throw new ArgumentNullException(nameof(velocityProvider));

            if (selfPositionProvider is null)
                throw new ArgumentNullException(nameof(selfPositionProvider));



            _data = data ?? throw new ArgumentNullException(nameof(data));
            _velocityUpdater = new(velocityProvider, UnityFrameProvider.FixedUpdate);
            _selfPositionUpdater = new(selfPositionProvider, UnityFrameProvider.FixedUpdate);



            IsMoving = _velocityUpdater.Property
                .Select(v => v.magnitude - NonMoovingThreshold.CurrentValue > 0)
                .ToReadOnlyReactiveProperty();



            var updateStream = Observable.EveryUpdate(UnityFrameProvider.PostFixedUpdate)
                .Where(_ => Direction.Value.HasValue)
                .Subscribe(_ =>
                {
                    if (_velocityUpdater.Property.CurrentValue.magnitude < MaxSpeed.CurrentValue)
                    {
                        var speedDifference = MaxSpeed.CurrentValue - _velocityUpdater.Property.CurrentValue.magnitude;
                        var acceleration = Mathf.Min(Speed.CurrentValue, speedDifference);
                        var force = Direction.CurrentValue.Value * acceleration;
                        _lastCalculatedForce.Value = force;
                        _lastCalculatedForce.ForceNotify();
                    }
                    else
                        _lastCalculatedVelocity.Value = _velocityUpdater.Property.CurrentValue.normalized * MaxSpeed.CurrentValue;
                });



            BuildPermanentDisposable(
                updateStream, _data, IsMoving, Direction, _selfPositionUpdater, _velocityUpdater,
                _lastCalculatedVelocity, _lastCalculatedForce, _lastCalculatedPosition
            );
        }



        public void Wrap(Vector3 newPosition)
        {
            ThrowIfDisposed();

            _lastCalculatedPosition.Value = newPosition;
        }
    }
}