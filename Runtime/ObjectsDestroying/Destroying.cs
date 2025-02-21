using System;
using System.Collections.Generic;
using R3;

namespace WhiteArrow.Incremental
{
    public static class Destroying
    {
        private static List<Timer> s_timers = new();





        #region Immediately
        public static void Destr(IDestroyable destroyable)
        {
            if (destroyable == null)
                throw new ArgumentNullException(nameof(destroyable));
            destroyable.Destroy();
        }
        #endregion



        #region Deley
        public static Observable<Unit> DelayDestr(IDestroyable destroyable, float time)
        {
            if (destroyable is null)
                throw new ArgumentNullException(nameof(destroyable));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time));


            if (time == 0)
            {
                Destr(destroyable);
                return Observable.ReturnUnit();
            }


            return LaunchDelayDestrTimer(time, destroyable);
        }


        public static Observable<Unit> DelayDestr(IDestroyable destroyable, DeleyDataAdapter deleyAdapter)
        {
            if (destroyable is null)
                throw new ArgumentNullException(nameof(destroyable));

            if (deleyAdapter == null)
                throw new NullReferenceException(nameof(deleyAdapter));


            if (deleyAdapter.ElapsedTime.Value >= deleyAdapter.Delay.Value)
            {
                Destr(destroyable);
                return Observable.ReturnUnit();
            }

            var stoppedTimer = GetStoppedTimer();
            IDisposable progressSubscription = null;

            var timerStream = stoppedTimer.Launch(deleyAdapter.Delay.CurrentValue)
                .ObserveOnMainThread()
                .Select(_ =>
                {
                    progressSubscription.Dispose();
                    Destr(destroyable);
                    return Unit.Default;
                });

            progressSubscription = stoppedTimer
                .GetProgress(p => deleyAdapter.ElapsedTime.Value = deleyAdapter.Delay.Value * p);

            return timerStream;
        }


        private static Observable<Unit> LaunchDelayDestrTimer(float time, IDestroyable destroyable)
        {
            if (destroyable is null)
                throw new ArgumentNullException(nameof(destroyable));

            var stoppedTimer = GetStoppedTimer();
            return stoppedTimer.Launch(time)
                .Select(_ =>
                {
                    Destr(destroyable);
                    return Unit.Default;
                });
        }



        private static Timer GetStoppedTimer()
        {
            var stoppedTimer = s_timers.Find(t => t.IsStopped.CurrentValue);
            if (stoppedTimer == null)
            {
                stoppedTimer = new();
                s_timers.Add(stoppedTimer);
            }

            return stoppedTimer;
        }
        #endregion



        #region Subscribed delay
        public static IDisposable SubscribedDelayDestr(IDestroyable destroyable, float time)
        {
            return DelayDestr(destroyable, time).Subscribe();
        }

        public static IDisposable SubscribedDelayDestr(IDestroyable destroyable, DeleyDataAdapter deleyAdapter)
        {
            return DelayDestr(destroyable, deleyAdapter).Subscribe();
        }
        #endregion
    }
}
