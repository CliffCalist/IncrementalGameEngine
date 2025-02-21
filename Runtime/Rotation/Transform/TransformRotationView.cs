using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class TransformRotationView : MonoBehaviour
    {
        public void Bind(TransformRotation viewModel)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var chachedTransform = transform;

            viewModel.LastCalculatedRotation
                .ObserveOnMainThread()
                .Subscribe(r => chachedTransform.rotation = r)
                .AddTo(this);
        }
    }
}