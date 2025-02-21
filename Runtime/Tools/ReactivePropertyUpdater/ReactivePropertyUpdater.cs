using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ReactivePropertyUpdater<T> : DisposableBase, IReactivePropertyUpdater
    {
        private readonly ReactiveProperty<T> _property = new();
        public ReadOnlyReactiveProperty<T> Property => _property;


        private Func<T> _getValue;

        private FrameProvider _frameProvider;
        private IDisposable _valueProvider;
        private IDisposable _updateSubscription;


        private ReactiveProperty<bool> _isPaused = new(true);
        public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused;



        public ReactivePropertyUpdater(Func<T> getValue, FrameProvider frameProvider, bool start = true)
        {
            if (frameProvider is null)
                throw new ArgumentNullException(nameof(frameProvider));

            _property.Value = getValue();
            SetValueGetter(getValue);
            ChangeFrameProvider(frameProvider);

            if (start)
                Start();

            BuildPermanentDisposable(_property, _isPaused);
        }

        public ReactivePropertyUpdater(MonoDataProvider<T> valueProvider, FrameProvider frameProvider, bool start = true)
        {
            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));
            if (frameProvider is null)
                throw new ArgumentNullException(nameof(frameProvider));

            _property.Value = valueProvider.Value;
            SetValueGetter(valueProvider);
            ChangeFrameProvider(frameProvider);

            if (start)
                Start();

            BuildPermanentDisposable(_property, _isPaused);
        }



        public void SetValueGetter(Func<T> getValue)
        {
            ThrowIfDisposed();
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }

        public void SetValueGetter(MonoDataProvider<T> valueProvider)
        {
            ThrowIfDisposed();

            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));

            _getValue = () => valueProvider.Value;
            _valueProvider = valueProvider;
        }



        public void ChangeFrameProvider(FrameProvider frameProvider)
        {
            ThrowIfDisposed();
            _frameProvider = frameProvider ?? throw new ArgumentNullException(nameof(frameProvider));
        }



        public void Stop(bool setDefault)
        {
            ThrowIfDisposed();

            if (!_isPaused.Value)
            {
                _updateSubscription?.Dispose();
                _isPaused.Value = true;

                if (setDefault)
                    _property.Value = default;
            }
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_isPaused.Value)
            {
                _isPaused.Value = false;
                Restart();
            }
        }

        public void Restart()
        {
            ThrowIfDisposed();

            _updateSubscription?.Dispose();
            _updateSubscription = Observable.EveryUpdate(_frameProvider)
                .Subscribe(_ =>
                {
                    if (_getValue == null)
                    {
                        Debug.LogWarning($"Value getter is null, maybe {nameof(ReactivePropertyUpdater<T>)} has been disposed.");
                        _updateSubscription?.Dispose();
                    }
                    else _property.Value = _getValue();
                });
        }



        public static implicit operator ReadOnlyReactiveProperty<T>(ReactivePropertyUpdater<T> updater)
        {
            return updater.Property;
        }



        protected override void DisposeProtected()
        {
            _updateSubscription?.Dispose();
            _valueProvider?.Dispose();
            _getValue = null;
            base.DisposeProtected();
        }
    }
}