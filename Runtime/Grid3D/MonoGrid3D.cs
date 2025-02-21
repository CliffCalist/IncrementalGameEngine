using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class MonoGrid3D : MonoBehaviour
    {
        [SerializeField] private Grid3D _grid;

        public Grid3D Grid => _grid;


        private void Awake()
        {
            _grid.OriginTransform = transform;
        }


        [ContextMenu(nameof(ApplyAutoScale))]
        public void ApplyAutoScale()
        {
            _grid.ApplyAutoScale();
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            _grid.OnDrawGizmos(transform);
        }
#endif
    }
}