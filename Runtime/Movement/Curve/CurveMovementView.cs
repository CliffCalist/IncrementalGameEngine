using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class CurveMovementView : MonoBehaviour
    {
        public void Bind(ICurveMovementProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var cahchedTransform = transform;
            provider.Movement.LastCalculatedSelfPosition
                .Where(p => p.HasValue)
                .ObserveOnMainThread()
                .Subscribe(p => cahchedTransform.position = p.Value)
                .AddTo(this);
        }
    }
}