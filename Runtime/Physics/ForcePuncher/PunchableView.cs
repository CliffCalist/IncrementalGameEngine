using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [RequireComponent(typeof(Rigidbody))]
    public class PunchableView : MonoBehaviour
    {
        public void Bind(IPunchable punchable)
        {
            if (punchable is null)
                throw new ArgumentNullException(nameof(punchable));

            var rigidbody = GetComponent<Rigidbody>();
            punchable.OnPunched
                .ObserveOnMainThread()
                .Subscribe(f => rigidbody.AddForce(f, ForceMode.Impulse))
                .AddTo(this);
        }
    }
}