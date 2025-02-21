using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class RigidbodyRotation : DisposableBase
    {
        private readonly RigidbodyRotationDataAdapter _dataAdapter;


        public ReactiveProperty<bool> IsEnabled => _dataAdapter.IsEnabled;
        public ReadOnlyReactiveProperty<float> Speed => _dataAdapter.Speed;

        public ReactiveProperty<Vector3?> TargetDirection { get; } = new(null);

        private readonly ReactivePropertyUpdater<Quaternion> _selfRotation;
        public ReadOnlyReactiveProperty<Quaternion> SelfRotation => _selfRotation;

        private readonly ReactiveProperty<Quaternion> _lastCalculatedRotation;
        public ReadOnlyReactiveProperty<Quaternion> LastCalculatedRotation => _lastCalculatedRotation;



        public RigidbodyRotation(RigidbodyRotationDataAdapter dataAdapter, MonoDataProvider<Quaternion> selfRotationProvider)
        {
            if (selfRotationProvider is null)
                throw new ArgumentNullException(nameof(selfRotationProvider));

            _dataAdapter = dataAdapter ?? throw new ArgumentNullException(nameof(dataAdapter));
            _selfRotation = new(selfRotationProvider, UnityFrameProvider.FixedUpdate);
            _lastCalculatedRotation = new(_selfRotation.Property.CurrentValue);


            var updateSubscription = Observable.EveryUpdate(UnityFrameProvider.PostFixedUpdate)
                .Where(_ => IsEnabled.CurrentValue && TargetDirection.CurrentValue.HasValue)
                .Subscribe(_ =>
                {
                    var targetRotation = Quaternion.LookRotation(TargetDirection.Value.Value);
                    var newRotation = Quaternion.RotateTowards(
                        _selfRotation.Property.CurrentValue,
                        targetRotation,
                        Speed.CurrentValue);

                    _lastCalculatedRotation.Value = newRotation;
                    _lastCalculatedRotation.ForceNotify();
                });



            BuildPermanentDisposable(updateSubscription, _dataAdapter, TargetDirection, _selfRotation, _lastCalculatedRotation);
        }
    }
}