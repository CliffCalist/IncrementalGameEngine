using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyRotationView : MonoBehaviour
    {
        public void Bind(RigidbodyRotation viewModel)
        {
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));

            var chachedRigidbody = GetComponent<Rigidbody>();

            viewModel.LastCalculatedRotation
                .ObserveOnMainThread()
                .Subscribe(r => chachedRigidbody.MoveRotation(r))
                .AddTo(this);
        }
    }
}