using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ViewSpawner<TView> : DisposableBase, IViewAsComponentSpawner, IViewSpawner<TView>
        where TView : Component
    {
        private WeightedRandomSelector<TView> _randomSelector;
        public int TotalWeight => _randomSelector.TotalWeight;

        private ReactiveProperty<int?> _maxSpawnCount;
        public ReadOnlyReactiveProperty<int?> MaxSpawnCount => _maxSpawnCount;

        private ReactiveProperty<int> _spawnedCount = new();
        public ReadOnlyReactiveProperty<int> SpawnedCount => _spawnedCount;


        public readonly ReactiveProperty<Transform> DefaultParent = new(null);
        public readonly ReactiveProperty<Vector3?> DefaultPosition = new(null);
        public readonly ReactiveProperty<Quaternion?> DefaultRotation = new(null);



        public ViewSpawner(ICollection<WeightedRandomVariant<TView>> prefabs, int? maxSpawnCount = null)
        {
            if (prefabs is null)
                throw new ArgumentNullException(nameof(prefabs));

            _randomSelector = new(prefabs.SelectByCondition(v => v != null));
            DefaultParent = new(null);
            DefaultPosition = new(null);
            DefaultRotation = new(null);
            _maxSpawnCount = new(maxSpawnCount);

            BuildPermanentDisposable(_maxSpawnCount, _spawnedCount, DefaultParent, DefaultPosition, DefaultRotation);
        }

        public ViewSpawner(ICollection<TView> prefabs, int? maxSpawnCount = null)
        {
            if (prefabs is null)
                throw new ArgumentNullException(nameof(prefabs));

            _randomSelector = new(prefabs.SelectByCondition(v => v != null));
            DefaultParent = new(null);
            DefaultPosition = new(null);
            DefaultRotation = new(null);
            _maxSpawnCount = new(maxSpawnCount);

            BuildPermanentDisposable(_maxSpawnCount, _spawnedCount, DefaultParent, DefaultPosition, DefaultRotation);
        }



        public ViewSpawner(WeightedRandomVariant<TView> prefab, int? maxSpawnCount = null)
        {
            if (prefab is null)
                throw new ArgumentNullException(nameof(prefab));

            _randomSelector = new(prefab);
            DefaultParent = new(null);
            DefaultPosition = new(null);
            DefaultRotation = new(null);
            _maxSpawnCount = new(maxSpawnCount);

            BuildPermanentDisposable(_maxSpawnCount, _spawnedCount, DefaultParent, DefaultPosition, DefaultRotation);
        }

        public ViewSpawner(TView prefab, int? maxSpawnCount = null)
        {
            if (prefab is null)
                throw new ArgumentNullException(nameof(prefab));

            _randomSelector = new(prefab);
            DefaultParent = new(null);
            DefaultPosition = new(null);
            DefaultRotation = new(null);
            _maxSpawnCount = new(maxSpawnCount);

            BuildPermanentDisposable(_maxSpawnCount, _spawnedCount, DefaultParent, DefaultPosition, DefaultRotation);
        }



        public void SetMaxSpawnCount(int? newCount)
        {
            ThrowIfDisposed();

            if (newCount != null || newCount < 0)
                throw new ArgumentOutOfRangeException(nameof(newCount));

            if (newCount < _spawnedCount.Value)
                throw new InvalidOperationException($"{nameof(MaxSpawnCount)} can't bee lees then {nameof(SpawnedCount)}");

            _maxSpawnCount.Value = newCount;
        }


        protected void PreSpawnValidation()
        {
            ThrowIfDisposed();

            if (_maxSpawnCount.Value > -1 && _spawnedCount.Value >= _maxSpawnCount.Value)
                throw new InvalidOperationException($"The maximum({MaxSpawnCount}) is reached.");
        }



        public virtual Observable<TView> SpawnByWeight(int weight, Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
        {
            ThrowIfDisposed();
            PreSpawnValidation();

            if (weight < 0 || weight >= TotalWeight)
                throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be within the valid range.");


            var prefab = _randomSelector.GetByWeight(weight);
            var finalParent = parent ?? DefaultParent.Value;
            var finalPosition = position ?? DefaultPosition.Value;
            var finalRotation = rotation ?? DefaultRotation.Value;

            var spawnedObject = UnityEngine.Object.Instantiate(
                prefab,
                finalPosition.HasValue ? finalPosition.Value : Vector3.zero,
                finalRotation.HasValue ? finalRotation.Value : Quaternion.identity,
                finalParent);

            _spawnedCount.Value++;
            return Observable.Return(spawnedObject);
        }

        public virtual Observable<Component> SpawnByWeightAsComponent(int weight, Transform parent, Vector3? position, Quaternion? rotation)
        {
            ThrowIfDisposed();

            return SpawnByWeight(weight, parent, position, rotation).Select(view => view as Component);
        }

        public virtual Observable<TView> Spawn(Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
        {
            ThrowIfDisposed();

            PreSpawnValidation();

            var randomWeight = UnityEngine.Random.Range(0, TotalWeight);
            return SpawnByWeight(randomWeight, parent, position, rotation);
        }
    }
}