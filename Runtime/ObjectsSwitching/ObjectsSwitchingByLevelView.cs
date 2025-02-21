using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ObjectsSwitchingByLevelView : MonoBehaviour
    {
        [SerializeField] private GameObject _currentObject;

        [Space]
        [SerializeField] private Transform _parent;
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private List<GameObject> _prefabsByLevel;


        private ReactiveProperty<GameObject> _currentObjectReactive = new();
        public ReadOnlyReactiveProperty<GameObject> CurrentObject => _currentObjectReactive;



        public void Bind(ILevelProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));


            _currentObjectReactive.Value = _currentObject;
            provider.Lvl.Value
                .Where(l => l <= _prefabsByLevel.Count)
                .ObserveOnMainThread()
                .Subscribe(l =>
                {
                    if (_currentObjectReactive.Value != null)
                        Destroy(_currentObjectReactive.Value);

                    _currentObjectReactive.Value = Instantiate(_prefabsByLevel[l - 1], _parent.position, Quaternion.Euler(_rotation), _parent);
                })
                .AddTo(this);


            _currentObjectReactive.AddTo(this);
            _currentObjectReactive
                .ObserveOnMainThread()
                .Subscribe(obj => _currentObject = obj)
                .AddTo(this);
        }
    }
}