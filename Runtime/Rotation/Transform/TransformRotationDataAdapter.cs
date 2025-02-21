using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class TransformRotationDataAdapter : DataAdapter
    {
        public ReactiveProperty<bool> IsEnabled { get; }
        public ReactiveProperty<float> Speed { get; }


        public TransformRotationDataAdapter(TransformRotationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            IsEnabled = new(data.IsEnabled);
            Speed = new(data.Speed);


            IsEnabled
                .Skip(1)
                .Subscribe(e => data.IsEnabled = e);

            Speed
                .Skip(1)
                .Subscribe(s => data.Speed = s);


            BuildPermanentChangesTracker(
                IsEnabled.AsUnitObservable().Skip(1),
                Speed.AsUnitObservable().Skip(1)
            );

            BuildPermanentDisposable(IsEnabled, Speed);
        }
    }
}