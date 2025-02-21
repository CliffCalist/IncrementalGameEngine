using System;
using System.Linq.Expressions;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LerpMovement : MovableByOneCall, IMovableWithOffset
    {
        private readonly LerpMovementDataAdapter _data;


        public override ReactiveProperty<bool> IsPaused => _data.IsPaused;

        public override ReadOnlyReactiveProperty<bool> IsMoving { get; }
        public override ReadOnlyReactiveProperty<bool> IsArrived { get; }


        public ReadOnlyReactiveProperty<float> Speed => _data.Speed;
        public ReactiveProperty<Vector3> Offset => _data.Offset;

        public ReadOnlyReactiveProperty<Vector3?> DesiredPosition { get; }

        private readonly ReactiveProperty<Vector3> _startPosition = new();
        public ReadOnlyReactiveProperty<Vector3> StartPosition => _startPosition;


        private readonly ReactiveProperty<float> _elaspedTime = new();
        public ReadOnlyReactiveProperty<float> ElaspedTime => _elaspedTime;

        public ReadOnlyReactiveProperty<float> Progress { get; }



        public LerpMovement(LerpMovementDataAdapter data, ReactivePropertyUpdater<Vector3> selfPosition)
            : base(selfPosition)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));


            Progress = _elaspedTime
                .CombineLatest(Speed, (elapsed, time) => Mathf.Clamp01(elapsed / time))
                .ToReadOnlyReactiveProperty();

            IsArrived = Progress
                .Select(p => p >= 1)
                .ToReadOnlyReactiveProperty();

            IsMoving = IsPaused
                .CombineLatest(IsArrived, (isPaused, isArrived) => !isPaused && !isArrived)
                .ToReadOnlyReactiveProperty();

            DesiredPosition = _targetPosition
                .Select(tp => tp.HasValue ? tp.Value + Offset.CurrentValue : (Vector3?)null)
                .ToReadOnlyReactiveProperty();



            _targetPosition
                .DistinctUntilChanged()
                .Where(p => p.HasValue)
                .Subscribe(tp =>
                {
                    _startPosition.Value = _selfPosition.Property.CurrentValue;
                    _elaspedTime.Value = 0;
                });



            var updateStream = Observable.EveryUpdate(UnityFrameProvider.PostFixedUpdate)
                .Where(_ => IsMoving.CurrentValue && DesiredPosition.CurrentValue.HasValue)
                .Subscribe(_ =>
                {
                    _elaspedTime.Value += Time.deltaTime;
                    _lastCalculatedSelfPosition.Value = Vector3.Lerp(
                        _startPosition.CurrentValue,
                        DesiredPosition.CurrentValue.Value,
                        Progress.CurrentValue);
                });



            BuildPermanentDisposable(
                updateStream, _data, IsMoving, IsArrived, DesiredPosition, _startPosition, _elaspedTime, Progress
            );
        }
    }
}