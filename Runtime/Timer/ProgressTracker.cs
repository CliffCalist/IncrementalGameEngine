using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ProgressTracker : DisposableBase
    {
        private readonly Timer _timer;
        private readonly Dictionary<FrameProvider, ReadOnlyReactiveProperty<float>> _progressCache = new();



        public ProgressTracker(Timer timer)
        {
            _timer = timer;
        }



        public Observable<float> GetProgress(FrameProvider frameProvider)
        {
            if (_progressCache.TryGetValue(frameProvider, out var cachedProgress))
                return cachedProgress;

            var progressObservable = _timer.IsStopped
                .DistinctUntilChanged()
                .Select(isStopped => isStopped
                    ? Observable.Return(_timer.IsBraked.CurrentValue ? CalculateProgress() : 1)
                    : Observable.EveryUpdate(frameProvider).Select(_ => CalculateProgress()))
                .Switch()
                .ToReadOnlyReactiveProperty();

            _progressCache[frameProvider] = progressObservable;
            return progressObservable;
        }

        private float CalculateProgress()
        {
            var elapsedTime = Time.time - _timer.StartTime;
            return Mathf.Clamp01(elapsedTime / _timer.Duration);
        }



        protected override void DisposeProtected()
        {
            foreach (var kvp in _progressCache)
                kvp.Value.Dispose();
            _progressCache.Clear();
        }
    }
}