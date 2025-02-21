using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LerpMovementView : MonoBehaviour
    {
        public void Bind(LerpMovement movement)
        {
            if (movement is null)
                throw new ArgumentNullException(nameof(movement));

            var chachedTransform = transform;
            movement.LastCalculatedSelfPosition
                .Where(p => p.HasValue)
                .ObserveOnMainThread()
                .Subscribe(p => chachedTransform.position = p.Value)
                .AddTo(this);
        }
    }
}