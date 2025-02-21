using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class Grid3D
    {
        #region Fields
        [SerializeField] private Transform _originTranform;
        [SerializeField] private Vector3 _offset;

        [Space]
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private Vector3Int _size;
        [SerializeField] private bool _invertZ;

        [Space]
        [SerializeField] private Vector3 _cellSize;
        public bool IsCellSizeMaxEnabled = false;
        [SerializeField] private Vector3 _maxCellSize;

        [Space]
        [SerializeField] private Vector3 _spacing;
        public bool IsSpacingMaxEnabled = false;
        [SerializeField] private Vector3 _maxSpacing;

        [Space]
        [SerializeField] private bool _autoScale;
        [SerializeField] private Vector3 _maxWorldSize;
        #endregion



        #region Prop: Grid sizes
        public Vector3Int Size
        {
            get => _size;
            set
            {
                _size = value;
                if (_autoScale)
                    ApplyAutoScale();
                else ChangedEvent?.Invoke();
            }
        }

        public Vector3 MaxWorldSize
        {
            get => _maxWorldSize;
            set
            {
                _maxWorldSize = value;
                if (_autoScale)
                    ApplyAutoScale();
                else ChangedEvent?.Invoke();
            }
        }


        public float ScaledWidth => (_size.x * ScaledCellSize.x) + ((_size.x - 1) * ScaledSpacing.x);
        public float ScaledDepth => (_size.z * ScaledCellSize.z) + ((_size.z - 1) * ScaledSpacing.z);
        public float ScaledHeight => (_size.y * ScaledCellSize.y) + ((_size.y - 1) * ScaledSpacing.y);
        public Vector3 ScaledWorldSize => new(ScaledWidth, ScaledHeight, ScaledDepth);

        public float Width => (_size.x * _cellSize.x) + ((_size.x - 1) * _spacing.x);
        public float Depth => (_size.z * _cellSize.z) + ((_size.z - 1) * _spacing.z);
        public float Height => (_size.y * _cellSize.y) + ((_size.y - 1) * _spacing.y);
        public Vector3 WorldSize => new(Width, Height, Depth);


        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                if (_autoScale)
                    ApplyAutoScale();
                else ChangedEvent?.Invoke();
            }
        }

        public float MaxWorldScale
        {
            get
            {
                var xScale = _maxWorldSize.x / Width;
                var yScale = _maxWorldSize.y / Height;
                var zScale = _maxWorldSize.z / Depth;

                return Min(xScale, yScale, zScale);
            }
        }
        #endregion



        #region Prop: Cells & Spacings
        public Vector3 CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = value;
                ChangedEvent?.Invoke();
            }
        }

        public Vector3 ScaledCellSize
        {
            get
            {
                var scale = ClampVectorAxes(Scale, MaxCellSizeScale);
                return Vector3.Scale(_cellSize, scale);
            }
        }

        public float MaxCellSizeScale
        {
            get
            {
                if (!IsCellSizeMaxEnabled)
                    return float.MaxValue;

                var xScale = _maxCellSize.x / _cellSize.x;
                var yScale = _maxCellSize.y / _cellSize.y;
                var zScale = _maxCellSize.z / _cellSize.z;

                return Min(xScale, yScale, zScale);
            }
        }



        public Vector3 Spacing
        {
            get => _spacing;
            set
            {
                _spacing = value;
                ChangedEvent?.Invoke();
            }
        }

        public Vector3 ScaledSpacing
        {
            get
            {
                var scale = ClampVectorAxes(Scale, MaxSpacingScale);
                return Vector3.Scale(_spacing, scale);
            }
        }

        public float MaxSpacingScale
        {
            get
            {
                if (!IsSpacingMaxEnabled)
                    return float.MaxValue;

                var xSpacing = _maxSpacing.x / _spacing.x;
                var ySpacing = _maxSpacing.y / _spacing.y;
                var zSpacing = _maxSpacing.z / _spacing.z;

                return Min(xSpacing, ySpacing, zSpacing);
            }
        }
        #endregion



        #region Prop: Common
        public Transform OriginTransform
        {
            get => _originTranform;
            set
            {
                _originTranform = value;
                ChangedEvent?.Invoke();
            }
        }

        public Vector3 Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                ChangedEvent?.Invoke();
            }
        }

        public bool InvertZ
        {
            get => _invertZ;
            set
            {
                _invertZ = value;
                ChangedEvent?.Invoke();
            }
        }



        public int Capacity => _size.x * _size.y * _size.z;

        public Vector3 LocalCenter => new Vector3(ScaledWidth, ScaledHeight, ScaledDepth) * 0.5f - ScaledCellSize / 2 - Offset;
        #endregion



        public event Action ChangedEvent;



        #region Constructors
        public Grid3D() { }

        public Grid3D(Grid3D template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            _originTranform = template._originTranform;

            _scale = template._scale;
            _size = template._size;
            _invertZ = template._invertZ;

            _cellSize = template._cellSize;
            IsCellSizeMaxEnabled = template.IsCellSizeMaxEnabled;
            _maxCellSize = template._maxCellSize;

            _spacing = template._spacing;
            IsSpacingMaxEnabled = template.IsSpacingMaxEnabled;
            _maxSpacing = template._maxSpacing;

            _autoScale = template._autoScale;
            _maxWorldSize = template._maxWorldSize;
        }
        #endregion



        #region Methods: Get pos in grid
        public Vector3Int GetGridPosition(int index)
        {
            var yIndex = index / (_size.x * _size.z);
            var xIndex = index / _size.z % _size.x;
            var zIndex = index % _size.z;

            if (_invertZ)
                zIndex = _size.z - 1 - zIndex;

            return new Vector3Int(xIndex, yIndex, zIndex);
        }



        public Vector3 GetWorldPosition(Vector3Int gridPosition)
        {
            if (_invertZ)
                gridPosition.z = _size.z - 1 - gridPosition.z;

            var xPos = gridPosition.x * (ScaledCellSize.x + ScaledSpacing.x);
            var yPos = gridPosition.y * (ScaledCellSize.y + ScaledSpacing.y);
            var zPos = gridPosition.z * (ScaledCellSize.z + ScaledSpacing.z);

            var localPosition = new Vector3(xPos, yPos, zPos) - LocalCenter;
            if (_originTranform != null)
                localPosition = _originTranform.position + _originTranform.rotation * localPosition;

            return localPosition;
        }

        public Vector3 GetWorldPosition(int index)
        {
            var gridPosition = GetGridPosition(index);
            return GetWorldPosition(gridPosition);
        }
        #endregion



        #region Methods: Math
        public float Min(params float[] valuesParams)
        {
            var min = float.MaxValue;
            foreach (var value in valuesParams)
            {
                if (value < min)
                    min = value;
            }

            return min;
        }

        public Vector3 ClampVectorAxes(Vector3 vector, float maxValue)
        {
            return new Vector3(
                Mathf.Min(vector.x, maxValue),
                Mathf.Min(vector.y, maxValue),
                Mathf.Min(vector.z, maxValue));
        }
        #endregion



        #region Methods: Gizmos
#if UNITY_EDITOR
        public void OnDrawGizmos(Transform originTransform = null)
        {
            if (originTransform != null)
                _originTranform = originTransform;

            Gizmos.color = Color.green;

            for (int i = 0; i < Capacity; i++)
            {
                var position = GetWorldPosition(i);
                Gizmos.DrawWireCube(position, ScaledCellSize);
            }


            if (_autoScale)
            {
                var center = _originTranform != null ? _originTranform.position : Vector3.zero;
                var maxWorldSizeWithRotation = _originTranform != null ? _originTranform.rotation * _maxWorldSize : _maxCellSize;

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, maxWorldSizeWithRotation);
            }

            Gizmos.color = Color.white;
        }
#endif
        #endregion




        #region Methods: Common
        public void ApplyAutoScale()
        {
            var worldScaleX = _maxWorldSize.x / WorldSize.x;
            var worldScaleY = _maxWorldSize.y / WorldSize.y;
            var worldScaleZ = _maxWorldSize.z / WorldSize.z;

            var scaleFactor = Min(worldScaleX, worldScaleY, worldScaleZ, MaxWorldScale, MaxCellSizeScale, MaxSpacingScale);
            _scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            ChangedEvent?.Invoke();
        }
        #endregion
    }
}