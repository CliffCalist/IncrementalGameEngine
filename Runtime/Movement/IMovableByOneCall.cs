using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IMovableByOneCall : IMovable
    {
        ReactiveProperty<bool> IsPaused { get; }
        ReadOnlyReactiveProperty<bool> IsArrived { get; }

        ReadOnlyReactiveProperty<Vector3> SelfPosition { get; }
        ReadOnlyReactiveProperty<Vector3?> LastCalculatedSelfPosition { get; }
        ReadOnlyReactiveProperty<Vector3?> TargetPosition { get; }



        void SetTarget(Vector3? target);
        void SetDynamicTarget(ReadOnlyReactiveProperty<Vector3> target, bool disposeIfArrived);
    }
}