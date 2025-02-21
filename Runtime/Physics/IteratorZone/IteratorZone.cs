using System;
using R3;
using R3.Triggers;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class IteratorZone : DisposableBase
    {
        private readonly ReactiveProperty<bool> _isPaused = new();
        public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused;


        private readonly IIteratable _itteratableTarget;
        private bool _isNextIterationAsNewProcess = true;

        private readonly ReactiveProperty<IMovable> _triggeredEntity = new();
        private IDisposable _entityMovingSubscription;


        private readonly Timer _timer;
        private readonly float _launchDelay;
        private readonly float _timeRate;





        public IteratorZone(Collider zone, IIteratable target, IteratorZoneSettings settings)
        {
            if (zone is null)
                throw new ArgumentNullException(nameof(zone));

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));



            _itteratableTarget = target ?? throw new ArgumentNullException(nameof(target));
            _launchDelay = settings.LaunchDelay;
            _timeRate = settings.TimeRate;



            var canProcessNextIteration = _isPaused
                .CombineLatest(
                    _triggeredEntity,
                    (isPaused, entity) => (isPaused, entity)
                )
                .Select(s =>
                {
                    if (s.isPaused || s.entity == null)
                        return Observable.Return(false);
                    else return s.entity.IsMoving.Select(s => !s);
                })
                .Switch()
                .ToReadOnlyReactiveProperty();

            _timer = new(canProcessNextIteration.Select(s => !s));



            canProcessNextIteration
                .CombineLatest(
                    _timer.IsStopped.ObserveOnMainThread(),
                    (canProcess, isTimerStopped) => (canProcess, isTimerStopped)
                )
                .Subscribe(s =>
                {
                    if (!s.canProcess || _timer.IsBraked.CurrentValue)
                        _isNextIterationAsNewProcess = true;

                    if (s.canProcess && s.isTimerStopped)
                        LaunchProccess();
                });


            var triggerEnterSubscription = zone.OnTriggerEnterAsObservable().Subscribe(OnTriggerEnter);
            var triggerStaySubscription = zone.OnTriggerStayAsObservable().Subscribe(OnTriggerStay);
            var triggerExitSubscription = zone.OnTriggerExitAsObservable().Subscribe(OnTriggerExit);


            BuildPermanentDisposable(
                triggerEnterSubscription, triggerStaySubscription, triggerExitSubscription,
                canProcessNextIteration, _timer, _isPaused
            );
        }



        public void Pause()
        {
            ThrowIfDisposed();

            if (!_isPaused.CurrentValue)
                _isPaused.Value = true;
        }

        public void Unpause()
        {
            ThrowIfDisposed();

            if (_isPaused.CurrentValue)
                _isPaused.Value = false;
        }



        private void OnTriggerEnter(Collider collider)
        {
            ThrowIfDisposed();
            OnEntityTriggered(collider);
        }

        private void OnTriggerStay(Collider collider)
        {
            ThrowIfDisposed();
            OnEntityTriggered(collider);
        }

        private void OnEntityTriggered(Collider collider)
        {
            ThrowIfDisposed();

            if (_triggeredEntity.CurrentValue != null)
                return;


            var entityProvider = collider.GetComponentInChildren<NonMonoDataProvider<IMovable>>(false);
            if (entityProvider != null && entityProvider.HasValue)
            {
                _triggeredEntity.Value = entityProvider.Value;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            ThrowIfDisposed();

            var entityProvider = collider.GetComponentInChildren<NonMonoDataProvider<IMovable>>(false);
            if (_triggeredEntity.CurrentValue != null && entityProvider.HasValue && _triggeredEntity.CurrentValue == entityProvider.Value)
            {
                _triggeredEntity.Value = null;
            }
        }



        private void LaunchProccess()
        {
            ThrowIfDisposed();

            if (_triggeredEntity.CurrentValue == null)
                return;

            if (_isNextIterationAsNewProcess)
            {
                _isNextIterationAsNewProcess = false;
                LaunchTimer(_launchDelay);
            }
            else LaunchTimer(_timeRate);
        }

        private void LaunchTimer(float time)
        {
            ThrowIfDisposed();

            _timer.Launch(time).Subscribe(_ => ProcessNextIteration());
        }

        private void ProcessNextIteration()
        {
            ThrowIfDisposed();

            _itteratableTarget.NextIteration();
            LaunchTimer(_timeRate);
        }



        protected override void DisposeProtected()
        {
            _entityMovingSubscription?.Dispose();
            base.DisposeProtected();
        }
    }
}