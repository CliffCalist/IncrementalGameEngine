using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class SmoothDampMovement : MovableByOneCall, IMovableWithOffset
    {
        private readonly SmoothDampMovementDataAdapter _data;


        public override ReactiveProperty<bool> IsPaused => _data.IsPaused;
        public override ReadOnlyReactiveProperty<bool> IsMoving { get; }
        public override ReadOnlyReactiveProperty<bool> IsArrived { get; }


        public ReadOnlyReactiveProperty<bool> ClampMaxSpeed => _data.ClampMaxSpeed;
        public ReadOnlyReactiveProperty<float> MaxSpeed => _data.MaxSpeed;

        public ReadOnlyReactiveProperty<bool> IsShouldDecreaseSpeed => _data.IsShouldDecreaseSpeed;
        public ReadOnlyReactiveProperty<float> Speed => _data.Time;

        public ReactiveProperty<Vector3> Offset => _data.Offset;
        public ReadOnlyReactiveProperty<float> ArrivedThreshold => _data.ArrivedThreshold;


        private readonly ReactiveProperty<Vector3> _velocity = new();
        public ReadOnlyReactiveProperty<Vector3> Velocity => _velocity;

        public ReadOnlyReactiveProperty<Vector3?> DesiredPosition { get; }
        public ReadOnlyReactiveProperty<float> DistanceToTarget { get; }



        public SmoothDampMovement(SmoothDampMovementDataAdapter data, MonoDataProvider<Vector3> selfPositionProvider)
            : base(new(selfPositionProvider, UnityFrameProvider.PreLateUpdate))
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));



            DesiredPosition = _targetPosition
                .CombineLatest(
                    Offset,
                    (position, offset) =>
                        position.HasValue ? position.Value + offset : (Vector3?)null)
                .ToReadOnlyReactiveProperty();

            DistanceToTarget = DesiredPosition
                .CombineLatest(
                    _selfPosition.Property,
                    (desiredPosition, position)
                        => desiredPosition.HasValue ? Vector3.Distance(desiredPosition.Value, position) : 0
                )
                .ToReadOnlyReactiveProperty();

            IsArrived = DistanceToTarget
                .CombineLatest(
                    ArrivedThreshold,
                    (distance, threshold) => distance - threshold <= 0)
                .ToReadOnlyReactiveProperty();

            IsMoving = IsPaused
                .CombineLatest(
                    Velocity,
                    (isPaused, velocity) => !isPaused && velocity.magnitude > 0)
                .ToReadOnlyReactiveProperty();



            var velocity = _velocity.CurrentValue;
            var updateStream = Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Where(_ => !IsArrived.CurrentValue && !IsPaused.Value)
                .Subscribe(_ =>
                {
                    var maxSpeed = ClampMaxSpeed.CurrentValue ? MaxSpeed.CurrentValue : float.MaxValue;
                    var smoothedPosition = Vector3.SmoothDamp(
                        _selfPosition.Property.CurrentValue,
                        DesiredPosition.CurrentValue.Value,
                        ref velocity,
                        Speed.CurrentValue,
                        maxSpeed,
                        Time.deltaTime);

                    _velocity.Value = velocity;
                    _lastCalculatedSelfPosition.Value = smoothedPosition;

                    if (IsShouldDecreaseSpeed.CurrentValue && Speed.CurrentValue > 0)
                        _data.Time.Value = Mathf.Max(0, Speed.CurrentValue - Time.fixedDeltaTime);
                });



            BuildPermanentDisposable(
                updateStream, _data, _velocity, DesiredPosition, DistanceToTarget, IsPaused, IsArrived, IsMoving
            );
        }
    }
}