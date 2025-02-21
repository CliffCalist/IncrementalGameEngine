using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class ForcePuncherData : IReadOnlyForcePuncherData
    {
        [Min(0)] public float Force;
        [Min(0)] public float Angle;

        float IReadOnlyForcePuncherData.Force => Force;
        float IReadOnlyForcePuncherData.Angle => Angle;



        public ForcePuncherData() { }

        public ForcePuncherData(IReadOnlyForcePuncherData template)
        {
            Force = template.Force;
            Angle = template.Angle;
        }
    }
}