using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class MovementAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _parameterName = DEFAULT_PARAMETER_NAME;

        public const string DEFAULT_PARAMETER_NAME = "isMoving";

        public void Bind(IMovableProvider viewModelProvider)
        {
            if (viewModelProvider == null)
                throw new ArgumentNullException(nameof(viewModelProvider));

            if (_animator == null)
                throw new NullReferenceException(nameof(_animator));

            viewModelProvider.Movement.IsMoving
                .Subscribe(m => _animator.SetBool(_parameterName, m))
                .AddTo(this);
        }
    }
}