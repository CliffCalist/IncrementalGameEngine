using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LookAt : DisposableBase, ILookAtProvider
    {
        private readonly ReactiveProperty<Quaternion> _rotation;
        public ReadOnlyReactiveProperty<Quaternion> Rotation => _rotation;

        private readonly ReactivePropertyUpdater<Quaternion> _targetRotation;
        public ReadOnlyReactiveProperty<Quaternion> TargetRotation => _targetRotation;


        LookAt ILookAtProvider.LookAt => this;



        public LookAt(MonoDataProvider<Quaternion> targetRotationProvider, FrameProvider frameProvider)
        {
            if (targetRotationProvider is null)
                throw new ArgumentNullException(nameof(targetRotationProvider));

            if (frameProvider is null)
                throw new ArgumentNullException(nameof(frameProvider));


            _targetRotation = new(targetRotationProvider, frameProvider);
            _rotation = new(_targetRotation.Property.CurrentValue);


            _targetRotation.Property
                .DistinctUntilChanged()
                .Subscribe(r => _rotation.Value = r);


            BuildPermanentDisposable(_targetRotation, _rotation);
        }

        public LookAt(FrameProvider frameProvider)
        {
            if (frameProvider is null)
                throw new ArgumentNullException(nameof(frameProvider));

            _targetRotation = new(() => Quaternion.identity, frameProvider);
            _rotation = new(_targetRotation.Property.CurrentValue);

            _targetRotation.Property
                .DistinctUntilChanged()
                .Subscribe(r => _rotation.Value = r);


            BuildPermanentDisposable(_targetRotation, _rotation);
        }



        public void SetTargetRotationProvider(MonoDataProvider<Quaternion> targetRotationProvider)
        {
            ThrowIfDisposed();

            _targetRotation.SetValueGetter(targetRotationProvider);
        }
    }
}