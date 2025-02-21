using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class SmoothDampMovementData : IReadOnlySmoothDampMovementData
    {
        [HideInInspector] public bool IsPaused;
        bool IReadOnlySmoothDampMovementData.IsPaused => IsPaused;



        public Vector3 Offset;
        Vector3 IReadOnlySmoothDampMovementData.Offset => Offset;



        [Min(0.001F)] public float ArrivedThreshold = 0.3F;
        float IReadOnlySmoothDampMovementData.ArrivedThreshold => ArrivedThreshold;



        public bool IsShouldDecreaseSpeed;
        bool IReadOnlySmoothDampMovementData.IsShouldDecreaseSpeed => IsShouldDecreaseSpeed;


        [Min(0.001F)] public float Time = 0.5F;
        float IReadOnlySmoothDampMovementData.Time => Time;

        public bool ClampMaxSpeed;
        bool IReadOnlySmoothDampMovementData.ClampMaxSpeed => ClampMaxSpeed;


        [Min(0.001F)] public float MaxSpeed = 5;
        float IReadOnlySmoothDampMovementData.MaxSpeed => MaxSpeed;



        public SmoothDampMovementData() { }

        public SmoothDampMovementData(IReadOnlySmoothDampMovementData tempalte)
        {
            if (tempalte is null)
                throw new ArgumentNullException(nameof(tempalte));

            IsPaused = tempalte.IsPaused;
            Offset = tempalte.Offset;
            ArrivedThreshold = tempalte.ArrivedThreshold;
            IsShouldDecreaseSpeed = tempalte.IsShouldDecreaseSpeed;
            Time = tempalte.Time;
            ClampMaxSpeed = tempalte.ClampMaxSpeed;
            MaxSpeed = tempalte.MaxSpeed;
        }
    }
}