using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class SmoothDampMovementView : MonoBehaviour
    {
        private SmoothDampMovement _movement;



        public void Bind(SmoothDampMovement movement)
        {
            _movement = movement;

            var transformLocal = transform;

            _movement.LastCalculatedSelfPosition
                .Where(p => p.HasValue)
                .ObserveOnMainThread()
                .Subscribe(p => transformLocal.position = p.Value)
                .AddTo(this);
        }


        private void OnDestroy()
        {
            _movement?.Dispose();
        }
    }
}