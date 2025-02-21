using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class TransformRotation : DisposableBase
    {
        private readonly TransformRotationDataAdapter _dataAdapter;


        public ReactiveProperty<bool> IsEnabled => _dataAdapter.IsEnabled;
        public ReactiveProperty<float> Speed => _dataAdapter.Speed;

        public ReactiveProperty<Vector3?> TargetDirection { get; } = new(null);

        private readonly ReactivePropertyUpdater<Quaternion> _selfRotationUpdater;
        public ReadOnlyReactiveProperty<Quaternion> SelfRotation => _selfRotationUpdater.Property;

        private readonly ReactiveProperty<Quaternion> _lastCalculatedRotation;
        public ReadOnlyReactiveProperty<Quaternion> LastCalculatedRotation => _lastCalculatedRotation;




        public TransformRotation(TransformRotationDataAdapter data, ReactivePropertyUpdater<Quaternion> selfRotation, FrameProvider updateProvider)
        {
            if (updateProvider is null)
                throw new ArgumentNullException(nameof(updateProvider));

            _dataAdapter = data ?? throw new ArgumentNullException(nameof(data));
            _selfRotationUpdater = selfRotation ?? throw new ArgumentNullException(nameof(selfRotation));


            _lastCalculatedRotation.Value = _selfRotationUpdater.Property.CurrentValue;


            var updateSubscription = Observable.EveryUpdate(updateProvider)
                .Where(_ => IsEnabled.Value && TargetDirection.CurrentValue.HasValue)
                .Subscribe(_ =>
                {
                    var newRotation = Quaternion.RotateTowards(
                        _selfRotationUpdater.Property.CurrentValue,
                        Quaternion.Euler(TargetDirection.CurrentValue.Value),
                        Speed.Value * Time.deltaTime);

                    _lastCalculatedRotation.Value = newRotation;
                });


            BuildPermanentDisposable(updateSubscription, _dataAdapter, _selfRotationUpdater, TargetDirection, _lastCalculatedRotation);
        }
    }
}