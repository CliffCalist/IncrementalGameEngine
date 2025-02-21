using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IPunchable : IDisposable
    {
        Observable<Vector3> OnPunched { get; }

        void Punch(Vector3 force);
    }
}