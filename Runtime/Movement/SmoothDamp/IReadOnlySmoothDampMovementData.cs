using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IReadOnlySmoothDampMovementData
    {
        bool IsPaused { get; }

        bool IsShouldDecreaseSpeed { get; }
        bool ClampMaxSpeed { get; }
        float ArrivedThreshold { get; }

        float Time { get; }
        float MaxSpeed { get; }

        Vector3 Offset { get; }
    }
}