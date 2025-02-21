using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class DeleyDataAdapter : DataAdapter
    {
        public ReactiveProperty<float> Delay { get; }
        public ReactiveProperty<float> ElapsedTime { get; }


        public DeleyDataAdapter(DelayData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));


            Delay = new(data.Delay);
            ElapsedTime = new(data.ElapsedTime);



            Delay
                .Skip(1)
                .Subscribe(t => data.Delay = t);

            ElapsedTime
                .Skip(1)
                .Subscribe(t => data.ElapsedTime = t);



            BuildPermanentChangesTracker(
                Delay.Skip(1).AsUnitObservable(),
                ElapsedTime.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(Delay, ElapsedTime);
        }
    }
}