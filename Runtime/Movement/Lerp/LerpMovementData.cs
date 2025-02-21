using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class LerpMovementData : IReadOnlyLerpMovementData
    {
        [HideInInspector] public bool IsPaused;

        [Min(0F)] public float Speed = 0.5F;
        public Vector3 Offset;

        bool IReadOnlyLerpMovementData.IsPaused => IsPaused;
        float IReadOnlyLerpMovementData.Speed => Speed;
        Vector3 IReadOnlyLerpMovementData.Offset => Offset;



        public LerpMovementData() { }

        public LerpMovementData(IReadOnlyLerpMovementData tempalte)
        {
            if (tempalte is null)
                throw new ArgumentNullException(nameof(tempalte));

            IsPaused = tempalte.IsPaused;
            Speed = tempalte.Speed;
            Offset = tempalte.Offset;
        }
    }
}