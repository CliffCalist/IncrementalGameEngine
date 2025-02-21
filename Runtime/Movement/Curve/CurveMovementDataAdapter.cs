using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class CurveMovementDataAdapter : DisposableBase
    {
        public ReactiveProperty<bool> IsPaused { get; }
        public ReactiveProperty<AnimationCurve> Trajectory { get; }
        public ReactiveProperty<float> TargetPositionThreshold { get; }


        public CurveMovementDataAdapter(CurveMovementData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            IsPaused = new(data.IsPaused);
            Trajectory = new(data.Trajectory);
            TargetPositionThreshold = new(data.TargetPositionThreshold);

            IsPaused
                .Skip(1)
                .Subscribe(v => data.IsPaused = v);

            Trajectory
                .Skip(1)
                .Subscribe(t => data.Trajectory = t);

            TargetPositionThreshold
                .Skip(1)
                .Subscribe(t => data.TargetPositionThreshold = t);


            BuildPermanentDisposable(IsPaused, Trajectory, TargetPositionThreshold);
        }
    }
}