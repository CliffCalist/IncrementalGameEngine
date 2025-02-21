using R3;

namespace WhiteArrow.Incremental
{
    public class RigidbodyMovementDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<float> NonMoovingThreshold;
        public readonly ReactiveProperty<float> MaxSpeed;
        public readonly ReactiveProperty<float> AccelerationSpeed;



        public RigidbodyMovementDataAdapter(RigidbodyMovementData data)
        {
            NonMoovingThreshold = new(data.NonMovingThreshold);
            MaxSpeed = new(data.MaxSpeed);
            AccelerationSpeed = new(data.AccelerationSpeed);


            NonMoovingThreshold
                .Skip(1)
                .Subscribe(t => data.NonMovingThreshold = t);

            MaxSpeed
                .Skip(1)
                .Subscribe(s => data.MaxSpeed = s);

            AccelerationSpeed
                .Skip(1)
                .Subscribe(s => data.AccelerationSpeed = s);


            BuildPermanentChangesTracker(
                NonMoovingThreshold.Skip(1).AsUnitObservable(),
                MaxSpeed.Skip(1).AsUnitObservable(),
                AccelerationSpeed.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(NonMoovingThreshold, MaxSpeed, AccelerationSpeed);
        }
    }
}