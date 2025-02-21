using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LookAtView : MonoBehaviour
    {
        public void Bind(ILookAtProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var cachedTransform = transform;
            provider.LookAt.Rotation
                .ObserveOnMainThread()
                .Subscribe(r => cachedTransform.rotation = r)
                .AddTo(this);
        }
    }
}