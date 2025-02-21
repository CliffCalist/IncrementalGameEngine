using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Timer : DisposableBase
    {
        public float Duration { get; private set; }
        public float StartTime { get; private set; }
        private IDisposable _timerSubscription;
        private readonly ProgressTracker _progressTracker;


        private readonly ReactiveProperty<bool> _isStopped = new(true);
        public ReadOnlyReactiveProperty<bool> IsStopped => _isStopped;

        private readonly Subject<Unit> _onStopped = new();
        public Observable<Unit> OnStopped => _onStopped;


        private IDisposable _brakeConditionSubscription;

        private readonly ReactiveProperty<bool> _isBraked = new();
        public ReadOnlyReactiveProperty<bool> IsBraked => _isBraked;


        private readonly Subject<Unit> _onBraked = new();
        public Observable<Unit> OnBraked => _onBraked;



        public Timer(Observable<bool> brakeCondition = null)
        {
            _progressTracker = new ProgressTracker(this);

            if (brakeCondition != null)
            {
                _brakeConditionSubscription = brakeCondition
                    .Where(s => s && !_isStopped.CurrentValue)
                    .Subscribe(_ => Brake());
            }

            BuildPermanentDisposable(_isStopped, _onBraked, _progressTracker);
        }


        /// <summary>
        /// Launches the timer and returns an Observable that fires when the timer completes.
        /// </summary>
        public Observable<Unit> Launch(float duration)
        {
            ThrowIfDisposed();

            if (!_isStopped.CurrentValue)
                return Observable.Empty<Unit>();

            _isBraked.Value = false;
            _isStopped.Value = false;
            Duration = duration;
            StartTime = Time.time;

            var completionSubject = new Subject<Unit>();
            _timerSubscription?.Dispose();

            _timerSubscription = Observable.Timer(TimeSpan.FromSeconds(Duration))
                .Subscribe(_ =>
                {
                    if (!_isStopped.CurrentValue && !_isDisposed)
                    {
                        completionSubject.OnNext(Unit.Default);
                        Stop();
                    }
                },
                _ => completionSubject.OnCompleted());

            return completionSubject;
        }



        public Observable<float> GetProgress()
        {
            ThrowIfDisposed();
            return _progressTracker.GetProgress(UnityFrameProvider.Update);
        }

        public IDisposable GetProgress(Action<float> progressUpdated)
        {
            ThrowIfDisposed();

            var progressSubscription = GetProgress().Subscribe(progressUpdated);
            return progressSubscription;
        }

        public Observable<float> GetProgress(FrameProvider frameProvider)
        {
            ThrowIfDisposed();
            return _progressTracker.GetProgress(frameProvider);
        }

        public IDisposable GetProgress(FrameProvider frameProvider, Action<float> progressUpdated)
        {
            ThrowIfDisposed();
            return GetProgress(frameProvider).Subscribe(progressUpdated);
        }



        public void Stop()
        {
            ThrowIfDisposed();

            if (_isStopped.CurrentValue)
                return;


            _isStopped.Value = true;
            _onStopped.OnNext(Unit.Default);
            _timerSubscription?.Dispose();
        }

        public void Brake()
        {
            ThrowIfDisposed();

            if (_isStopped.CurrentValue)
                return;


            _isBraked.Value = true;
            _onBraked.OnNext(Unit.Default);
            Stop();
        }



        protected override void DisposeProtected()
        {
            _brakeConditionSubscription?.Dispose();
            _timerSubscription?.Dispose();
            base.DisposeProtected();
        }
    }
}