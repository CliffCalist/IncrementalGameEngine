using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class LevitationDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<float> Speed;
        public readonly ReactiveProperty<float> Height;


        public LevitationDataAdapter(LevitationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Speed = new(data.Speed);
            Height = new(data.Height);


            Speed
                .Skip(1)
                .Subscribe(s => data.Speed = s);

            Height
                .Skip(1)
                .Subscribe(h => data.Height = h);


            BuildPermanentChangesTracker(
                Speed.Skip(1).AsUnitObservable(),
                Height.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(Speed, Height);
        }
    }
}