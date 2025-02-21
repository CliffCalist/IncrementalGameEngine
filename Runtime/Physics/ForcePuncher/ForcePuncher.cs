using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WhiteArrow.Incremental
{
    public class ForcePuncher : DisposableBase
    {
        private readonly ForcePuncherDataAdapter _data;



        public ForcePuncher(ForcePuncherDataAdapter data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            BuildPermanentDisposable(_data);
        }


        public void Punch(IPunchable target)
        {
            ThrowIfDisposed();

            var randomDirection = Random.insideUnitSphere.normalized;
            var direction = new Vector3(
                randomDirection.x,
                Mathf.Tan(_data.Angle.CurrentValue * Mathf.Deg2Rad),
                randomDirection.y).normalized;

            var force = direction * _data.Force.CurrentValue;
            target.Punch(force);
        }
    }
}