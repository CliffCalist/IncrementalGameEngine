using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MovementParticlesView : MonoBehaviour
    {
        public void Bind(IMovable viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var particle = GetComponent<ParticleSystem>();

            viewModel.IsMoving
                .Subscribe(m =>
                {
                    var emmision = particle.emission;
                    emmision.enabled = m;
                })
                .AddTo(this);
        }
    }
}