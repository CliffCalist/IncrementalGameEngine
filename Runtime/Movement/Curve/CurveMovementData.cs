using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class CurveMovementData : IReadOnlyCurveMovementData
    {
        [HideInInspector] public bool IsPaused;
        public AnimationCurve Trajectory;
        [Min(0)] public float TargetPositionThreshold = 0.3F;


        bool IReadOnlyCurveMovementData.IsPaused => IsPaused;
        AnimationCurve IReadOnlyCurveMovementData.Trajectory => Trajectory;


        public CurveMovementData(AnimationCurve trajectory)
        {
            Trajectory = trajectory ?? throw new ArgumentNullException(nameof(trajectory));
        }

        public CurveMovementData(IReadOnlyCurveMovementData template)
        {
            IsPaused = template.IsPaused;
            Trajectory = new AnimationCurve();

            // Copy by new instance because AnimationCurve is class(referenced type)
            Trajectory.CopyFrom(template.Trajectory);
        }
    }
}