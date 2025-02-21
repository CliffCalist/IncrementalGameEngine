using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class LevitationData : IReadOnlyLevitationData
    {
        [Min(0)] public float Speed;
        [Min(0)] public float Height;

        float IReadOnlyLevitationData.Speed => Speed;
        float IReadOnlyLevitationData.Height => Height;



        public LevitationData() { }

        public LevitationData(IReadOnlyLevitationData tempalte)
        {
            if (tempalte is null)
                throw new ArgumentNullException(nameof(tempalte));

            Speed = tempalte.Speed;
            Height = tempalte.Height;
        }
    }
}