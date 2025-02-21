using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class RigidbodyRotationData : IReadOnlyRigidbodyRotationData
    {
        [HideInInspector] public bool IsEnabled;
        public float Speed;


        bool IReadOnlyRigidbodyRotationData.IsEnabled => IsEnabled;
        float IReadOnlyRigidbodyRotationData.Speed => Speed;


        public RigidbodyRotationData() { }

        public RigidbodyRotationData(IReadOnlyRigidbodyRotationData template)
        {
            IsEnabled = template.IsEnabled;
            Speed = template.Speed;
        }
    }
}
