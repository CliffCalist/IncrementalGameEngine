using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class NavMeshMovementData : IReadOnlyNavMeshMovementData
    {
        [HideInInspector] public bool IsPaused;

        public float Speed;
        public float Acceleration;
        public float RotationSpeed;

        bool IReadOnlyNavMeshMovementData.IsPaused => IsPaused;
        float IReadOnlyNavMeshMovementData.Speed => Speed;
        float IReadOnlyNavMeshMovementData.Acceleration => Acceleration;
        float IReadOnlyNavMeshMovementData.RotationSpeed => RotationSpeed;


        public NavMeshMovementData() { }

        public NavMeshMovementData(IReadOnlyNavMeshMovementData template)
        {
            IsPaused = template.IsPaused;
            Speed = template.Speed;
            Acceleration = template.Acceleration;
            RotationSpeed = template.RotationSpeed;
        }
    }
}