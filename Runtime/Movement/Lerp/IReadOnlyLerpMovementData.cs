using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IReadOnlyLerpMovementData
    {
        public bool IsPaused { get; }

        public float Speed { get; }
        public Vector3 Offset { get; }
    }
}