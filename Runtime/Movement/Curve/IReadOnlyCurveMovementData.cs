using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IReadOnlyCurveMovementData
    {
        public bool IsPaused { get; }
        public AnimationCurve Trajectory { get; }
    }
}