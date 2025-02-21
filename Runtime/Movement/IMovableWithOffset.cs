using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IMovableWithOffset : IMovable
    {
        public ReactiveProperty<Vector3> Offset { get; }
        public ReadOnlyReactiveProperty<Vector3?> DesiredPosition { get; }
    }
}
