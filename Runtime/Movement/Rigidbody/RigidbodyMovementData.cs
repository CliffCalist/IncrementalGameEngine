using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class RigidbodyMovementData : IReadOnlyRigidbodyMovementData
    {
        public float NonMovingThreshold;

        public float MaxSpeed;
        public float AccelerationSpeed;


        float IReadOnlyRigidbodyMovementData.NonMovingThreshold => NonMovingThreshold;
        float IReadOnlyRigidbodyMovementData.MaxSpeed => MaxSpeed;
        float IReadOnlyRigidbodyMovementData.AccelerationSpeed => AccelerationSpeed;


        public RigidbodyMovementData() { }

        public RigidbodyMovementData(IReadOnlyRigidbodyMovementData template)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            NonMovingThreshold = template.NonMovingThreshold;
            MaxSpeed = template.MaxSpeed;
            AccelerationSpeed = template.AccelerationSpeed;
        }
    }
}
