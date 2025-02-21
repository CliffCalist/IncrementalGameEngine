namespace WhiteArrow.Incremental
{
    public interface IReadOnlyRigidbodyMovementData
    {
        public float NonMovingThreshold { get; }

        public float MaxSpeed { get; }
        public float AccelerationSpeed { get; }
    }
}