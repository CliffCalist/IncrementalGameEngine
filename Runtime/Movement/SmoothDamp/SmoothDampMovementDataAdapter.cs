using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class SmoothDampMovementDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<bool> IsPaused;
        public readonly ReactiveProperty<bool> IsShouldDecreaseSpeed;

        public readonly ReactiveProperty<bool> ClampMaxSpeed;
        public readonly ReactiveProperty<float> MaxSpeed;
        public readonly ReactiveProperty<float> Time;

        public readonly ReactiveProperty<Vector3> Offset;
        public readonly ReactiveProperty<float> ArrivedThreshold;



        public SmoothDampMovementDataAdapter(SmoothDampMovementData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            IsPaused = new(data.IsPaused);
            IsShouldDecreaseSpeed = new(data.IsShouldDecreaseSpeed);
            ClampMaxSpeed = new(data.ClampMaxSpeed);
            ArrivedThreshold = new(data.ArrivedThreshold);
            Time = new(data.Time);
            MaxSpeed = new(data.MaxSpeed);
            Offset = new(data.Offset);


            IsPaused
                .Skip(1)
                .Subscribe(p => data.IsPaused = p);

            IsShouldDecreaseSpeed.
                Skip(1)
                .Subscribe(d => data.IsShouldDecreaseSpeed = d);

            ClampMaxSpeed
                .Skip(1)
                .Subscribe(c => data.ClampMaxSpeed = c);

            ArrivedThreshold
                .Skip(1)
                .Subscribe(t => data.ArrivedThreshold = t);

            Offset
                .Skip(1)
                .Subscribe(o => data.Offset = o);

            Time
                .Skip(1)
                .Subscribe(t => data.Time = t);

            MaxSpeed
                .Skip(1)
                .Subscribe(s => data.MaxSpeed = s);


            BuildPermanentChangesTracker(
                IsPaused.AsUnitObservable().Skip(1),
                IsShouldDecreaseSpeed.AsUnitObservable().Skip(1),
                ClampMaxSpeed.AsUnitObservable().Skip(1),
                ArrivedThreshold.AsUnitObservable().Skip(1),
                Time.AsUnitObservable().Skip(1),
                MaxSpeed.AsUnitObservable().Skip(1),
                Offset.AsUnitObservable().Skip(1)
            );

            BuildPermanentDisposable(
                IsPaused, IsShouldDecreaseSpeed, ClampMaxSpeed, ArrivedThreshold,
                Time, MaxSpeed, Offset
            );
        }
    }
}