using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LerpMovementDataAdapter : DisposableBase
    {
        public readonly ReactiveProperty<bool> IsPaused;
        public readonly ReactiveProperty<float> Speed;
        public readonly ReactiveProperty<Vector3> Offset;



        public LerpMovementDataAdapter(LerpMovementData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            IsPaused = new(data.IsPaused);
            Speed = new(data.Speed);
            Offset = new(data.Offset);


            IsPaused
                .Skip(1)
                .Subscribe(p => data.IsPaused = p);

            Speed
                .Skip(1)
                .Subscribe(s => data.Speed = s);

            Offset
                .Skip(1)
                .Subscribe(o => data.Offset = o);


            BuildPermanentDisposable(IsPaused, Speed, Offset);
        }
    }
}