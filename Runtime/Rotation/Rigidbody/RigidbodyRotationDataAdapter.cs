using R3;

namespace WhiteArrow.Incremental
{
    public class RigidbodyRotationDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<bool> IsEnabled;
        public readonly ReactiveProperty<float> Speed;



        public RigidbodyRotationDataAdapter(RigidbodyRotationData data)
        {
            IsEnabled = new(data.IsEnabled);
            Speed = new(data.Speed);


            IsEnabled.Skip(1)
                .Subscribe(e => data.IsEnabled = e);
            Speed.Skip(1)
                .Subscribe(s => data.Speed = s);


            BuildPermanentChangesTracker(
                IsEnabled.Skip(1).AsUnitObservable(),
                Speed.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(IsEnabled, Speed);
        }
    }
}