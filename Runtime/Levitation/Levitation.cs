using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Levitation : DisposableBase
    {
        private readonly LevitationDataAdapter _data;

        public ReadOnlyReactiveProperty<float> Speed => _data.Speed;
        public ReadOnlyReactiveProperty<float> Height => _data.Height;

        private ReactiveProperty<Vector3> _startLocalPosition = new();
        public ReadOnlyReactiveProperty<Vector3> StartLocalPosition => _startLocalPosition;

        private readonly ReactiveProperty<Vector3> _lastCalculatedLocalPosition;
        public ReadOnlyReactiveProperty<Vector3> LocalPosition => _lastCalculatedLocalPosition;



        public Levitation(LevitationDataAdapter data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));

            _lastCalculatedLocalPosition = new(CalculateSelfPosition());
            var updateSubscription = Observable.EveryUpdate()
                .Subscribe(_ => _lastCalculatedLocalPosition.Value = CalculateSelfPosition());

            BuildPermanentDisposable(
                updateSubscription, _data, _startLocalPosition, _lastCalculatedLocalPosition
            );
        }

        public Levitation(LevitationDataAdapter dataAdapter, Vector3 startLocalPosition)
        {
            _data = dataAdapter ?? throw new ArgumentNullException(nameof(dataAdapter));

            _startLocalPosition.Value = startLocalPosition;
            _lastCalculatedLocalPosition = new(CalculateSelfPosition());
            var updateSubscription = Observable.EveryUpdate()
                .Subscribe(_ => _lastCalculatedLocalPosition.Value = CalculateSelfPosition());

            BuildPermanentDisposable(
                updateSubscription, _data, _startLocalPosition, _lastCalculatedLocalPosition
            );
        }



        public void SetStartLocalPosition(Vector3 newPosition)
        {
            ThrowIfDisposed();

            _startLocalPosition.Value = newPosition;
        }



        private Vector3 CalculateSelfPosition()
        {
            ThrowIfDisposed();

            var newY = Mathf.PingPong(
                Time.time * Speed.CurrentValue,
                Height.CurrentValue * 2) - Height.CurrentValue;

            return new(
                _startLocalPosition.Value.x,
                _startLocalPosition.Value.y + newY,
                _startLocalPosition.Value.z);
        }
    }
}