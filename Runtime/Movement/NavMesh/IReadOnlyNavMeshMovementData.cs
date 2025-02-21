namespace WhiteArrow.Incremental
{
    public interface IReadOnlyNavMeshMovementData
    {
        public bool IsPaused { get; }

        public float Speed { get; }
        public float Acceleration { get; }
        public float RotationSpeed { get; }
    }
}