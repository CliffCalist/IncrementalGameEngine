using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IMovable : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsMoving { get; }


        public void Wrap(Vector3 newPosition);
    }
}