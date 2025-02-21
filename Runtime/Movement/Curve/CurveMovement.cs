using System;
using R3;
using UnityEditor;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class CurveMovement : MovableByOneCall, ICurveMovementProvider
    {
        private readonly CurveMovementDataAdapter _data;


        public override ReactiveProperty<bool> IsPaused => _data.IsPaused;

        public override ReadOnlyReactiveProperty<bool> IsMoving { get; }
        public override ReadOnlyReactiveProperty<bool> IsArrived { get; }


        public ReadOnlyReactiveProperty<AnimationCurve> Trajectory => _data.Trajectory;
        public ReadOnlyReactiveProperty<float> Speed { get; }
        public ReadOnlyReactiveProperty<float> TargetPositionThreshold => _data.TargetPositionThreshold;


        private readonly ReactiveProperty<Vector3> _startPosition = new();
        public ReadOnlyReactiveProperty<Vector3> StartPosition => _startPosition;

        private Vector3? _lastTargetPosition;

        private readonly ReactiveProperty<float> _elapsedTime = new();
        public ReadOnlyReactiveProperty<float> ElapsedTime => _elapsedTime;

        public ReadOnlyReactiveProperty<float> Progress { get; }


        CurveMovement ICurveMovementProvider.Movement => this;



        public CurveMovement(CurveMovementDataAdapter data, ReactivePropertyUpdater<Vector3> selfPosition)
            : base(selfPosition)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));


            Speed = Trajectory
                .Select(t => t.keys.Length == 0 ? 0 : t.keys[t.length - 1].time)
                .ToReadOnlyReactiveProperty();

            Progress = ElapsedTime
                .Select(t => t == 0 ? 0 : t / Speed.CurrentValue)
                .ToReadOnlyReactiveProperty();

            IsArrived = Progress
                .Select(p => p >= 1)
                .ToReadOnlyReactiveProperty();

            IsMoving = Progress
                .Select(p => p < 1)
                .ToReadOnlyReactiveProperty();


            _targetPosition
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    if (p.HasValue)
                    {
                        var needRestartTrajectory = !_lastTargetPosition.HasValue ||
                            Vector3.Distance(_lastTargetPosition.Value, p.Value) > TargetPositionThreshold.CurrentValue;

                        if (needRestartTrajectory)
                        {
                            _startPosition.Value = _selfPosition.Property.CurrentValue;
                            _elapsedTime.Value = 0;
                        }
                    }
                    else _elapsedTime.Value = 1;

                    _lastTargetPosition = p;
                });



            var updateStream = Observable.EveryUpdate()
                .Where(_ => !IsPaused.Value && !IsArrived.CurrentValue && _targetPosition.CurrentValue.HasValue)
                .Subscribe(_ =>
                {
                    try
                    {
                        _elapsedTime.Value += Time.deltaTime;
                        var currentPosition = Vector3.Lerp(
                            StartPosition.CurrentValue,
                            _targetPosition.CurrentValue.Value,
                            Progress.CurrentValue);

                        var yOffset = Trajectory.CurrentValue.Evaluate(ElapsedTime.CurrentValue);
                        currentPosition.y += yOffset;
                        _lastCalculatedSelfPosition.Value = currentPosition;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                });


            BuildPermanentDisposable(
                updateStream, IsMoving, IsArrived, Progress, Speed, _data, _startPosition, _elapsedTime
            );
        }
    }
}