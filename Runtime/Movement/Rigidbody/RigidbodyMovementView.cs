using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMovementView : MonoBehaviour
    {
        private RigidbodyMovement _movement;
        private Rigidbody _rigidbody;
        private Transform _transform;

        public void Bind(RigidbodyMovement movement)
        {
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;

            _movement.LastCalculatedForce
                .Skip(1)
                .ObserveOnMainThread()
                .Subscribe(d => _rigidbody.AddForce(d, ForceMode.Acceleration))
                .AddTo(this);

            _movement.LastCalculatedVelocity
                .Skip(1)
                .ObserveOnMainThread()
                .Subscribe(v => _rigidbody.velocity = v)
                .AddTo(this);

            _movement.LastCalculatedPosition
                .Skip(1)
                .ObserveOnMainThread()
                .Subscribe(p => _transform.position = p)
                .AddTo(this);
        }


        private void OnDestroy()
        {
            _movement?.Dispose();
        }
    }
}