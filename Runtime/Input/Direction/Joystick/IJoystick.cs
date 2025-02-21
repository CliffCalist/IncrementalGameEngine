using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IJoystick : IDisposable
    {
        public ReadOnlyReactiveProperty<Vector2?> Direction { get; }

        public void BrakeInput();
    }
}