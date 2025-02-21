using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LevitationView : MonoBehaviour
    {
        public void Bind(Levitation viewModel)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var cachedTransform = transform;

            viewModel.LocalPosition
                .ObserveOnMainThread()
                .Subscribe(p => cachedTransform.localPosition = p)
                .AddTo(this);
        }
    }
}