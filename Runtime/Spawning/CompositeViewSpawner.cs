using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class CompositeViewSpawner : DisposableBase, IViewAsComponentSpawner
    {
        private readonly List<IViewAsComponentSpawner> _spawners = new();

        public int TotalWeight => _spawners.Sum(spawner => spawner.TotalWeight);



        public CompositeViewSpawner(List<IViewAsComponentSpawner> spawners = null)
        {
            if (spawners != null)
            {
                ValidateSpawners(spawners);
                _spawners = spawners;
            }
        }

        private void ValidateSpawners(List<IViewAsComponentSpawner> spawners)
        {
            ThrowIfDisposed();

            if (spawners == null || spawners.Count == 0)
                throw new ArgumentException($"{nameof(spawners)} cannot be null or empty.");

            foreach (var spawner in spawners)
            {
                if (spawner == null || spawner.TotalWeight <= 0)
                    throw new ArgumentException("Invalid spawner.");
            }
        }



        public void AddSpawner(IViewAsComponentSpawner spawner)
        {
            ThrowIfDisposed();

            if (spawner == null || spawner.TotalWeight <= 0)
                throw new ArgumentException("Invalid spawner or weight.");

            _spawners.Add(spawner);
        }



        private void PreSpawnValidation()
        {
            ThrowIfDisposed();

            if (_spawners.Count == 0)
                throw new InvalidOperationException("No spawners available for composite spawning.");
        }

        public Observable<Component> SpawnByWeightAsComponent(int weight, Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
        {
            ThrowIfDisposed();
            PreSpawnValidation();

            int accumulatedWeight = 0;
            foreach (var spawner in _spawners)
            {
                accumulatedWeight += spawner.TotalWeight;
                if (weight < accumulatedWeight)
                {
                    return spawner.SpawnByWeightAsComponent(weight, parent, position, rotation);
                }
            }

            throw new InvalidOperationException("No valid spawner was found in the list.");
        }

        public Observable<T> Spawn<T>(Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
            where T : Component
        {
            ThrowIfDisposed();
            return SpawnByWeightAsComponent(UnityEngine.Random.Range(0, TotalWeight), parent, position, rotation)
                    .Select(component =>
                    {
                        if (component is T castedObject)
                            return castedObject;
                        throw new InvalidCastException($"Spawned object is not of type {typeof(T)}.");
                    });
        }


        protected override void DisposeProtected()
        {
            base.DisposeProtected();

            foreach (var spawner in _spawners)
                spawner.Dispose();
            _spawners.Clear();
        }
    }
}