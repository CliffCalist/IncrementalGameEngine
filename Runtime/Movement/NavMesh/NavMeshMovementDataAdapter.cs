using R3;

namespace WhiteArrow.Incremental
{
    public class NavMeshMovementDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<bool> IsPaused;
        public readonly ReactiveProperty<float> Speed;
        public readonly ReactiveProperty<float> Acceleration;
        public readonly ReactiveProperty<float> RotationSpeed;



        public NavMeshMovementDataAdapter(NavMeshMovementData data)
        {
            IsPaused = new(data.IsPaused);
            Speed = new(data.Speed);
            Acceleration = new(data.Acceleration);
            RotationSpeed = new(data.RotationSpeed);


            IsPaused.Skip(1).Subscribe(p => data.IsPaused = p);
            Speed.Skip(1).Subscribe(s => data.Speed = s);
            Acceleration.Skip(1).Subscribe(a => data.Acceleration = a);
            RotationSpeed.Skip(1).Subscribe(s => data.RotationSpeed = s);


            BuildPermanentChangesTracker(
                IsPaused.Skip(1).AsUnitObservable(),
                Speed.Skip(1).AsUnitObservable(),
                Acceleration.Skip(1).AsUnitObservable(),
                RotationSpeed.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(IsPaused, Speed, Acceleration, RotationSpeed);
        }
    }
}