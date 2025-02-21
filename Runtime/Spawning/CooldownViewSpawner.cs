using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class CooldownViewSpawner<T> : ViewSpawner<T>, IDisposable
        where T : Component
    {
        private readonly Queue<SpawnRequest> _spawnQueue = new();
        private readonly ReactiveProperty<bool> _isSpawning = new(false);


        private readonly ReactiveProperty<float> _cooldown;
        public ReadOnlyReactiveProperty<float> Cooldown => _cooldown;

        private IDisposable _currentSpawnTimer;



        public CooldownViewSpawner(ICollection<WeightedRandomVariant<T>> prefabs, float cooldown, int? maxSpawnCount = null)
            : base(prefabs, maxSpawnCount)
        {
            if (cooldown <= 0)
                throw new ArgumentOutOfRangeException(nameof(cooldown));

            _cooldown = new(cooldown);
            BuildPermanentDisposable(_isSpawning, _cooldown);
        }

        public CooldownViewSpawner(ICollection<T> prefabs, float cooldown, int? maxSpawnCount = null)
            : base(prefabs, maxSpawnCount)
        {
            if (cooldown <= 0)
                throw new ArgumentOutOfRangeException(nameof(cooldown));

            _cooldown = new(cooldown);
            BuildPermanentDisposable(_isSpawning, _cooldown);
        }



        public CooldownViewSpawner(WeightedRandomVariant<T> prefab, float cooldown, int? maxSpawnCount = null)
            : base(prefab, maxSpawnCount)
        {
            if (cooldown <= 0)
                throw new ArgumentOutOfRangeException(nameof(cooldown));

            _cooldown = new(cooldown);
            BuildPermanentDisposable(_isSpawning, _cooldown);
        }

        public CooldownViewSpawner(T prefab, float cooldown, int? maxSpawnCount = null)
            : base(prefab, maxSpawnCount)
        {
            if (cooldown <= 0)
                throw new ArgumentOutOfRangeException(nameof(cooldown));

            _cooldown = new(cooldown);
            BuildPermanentDisposable(_isSpawning, _cooldown);
        }



        /// <summary>
        /// Adds a cooldown delay without executing a spawn.
        /// Useful for mechanics where a delay is required before the first spawn.
        /// </summary>
        public void AddCooldown()
        {
            ThrowIfDisposed();

            if (_isSpawning.Value)
                return;

            _isSpawning.Value = true;

            _currentSpawnTimer = Observable
                .Timer(TimeSpan.FromSeconds(_cooldown.Value))
                .Subscribe(_ =>
                {
                    _isSpawning.Value = false;
                    ProcessQueue();
                });
        }



        public override Observable<Component> SpawnByWeightAsComponent(int weight, Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
        {
            ThrowIfDisposed();
            PreSpawnValidation();

            var completion = new Subject<Component>();
            var spawnRequest = new SpawnRequest(
                () => base.SpawnByWeightAsComponent(weight, parent, position, rotation),
                completion.AsObserver());

            _spawnQueue.Enqueue(spawnRequest);
            ProcessQueue();

            return completion.AsObservable();
        }

        public override Observable<T> Spawn(Transform parent = null, Vector3? position = null, Quaternion? rotation = null)
        {
            ThrowIfDisposed();

            var completion = new Subject<Component>();
            var spawnRequest = new SpawnRequest(
                () => base.Spawn(parent, position, rotation).Select(c => c as Component),
                completion.AsObserver());

            _spawnQueue.Enqueue(spawnRequest);
            ProcessQueue();

            return completion.Select(c => c as T);
        }

        private void ProcessQueue()
        {
            ThrowIfDisposed();

            if (_isSpawning.Value || _spawnQueue.Count == 0)
                return;

            _isSpawning.Value = true;

            var spawnRequest = _spawnQueue.Dequeue();

            if (_cooldown.Value <= 0)
                SpawnNow(spawnRequest);
            else
                _currentSpawnTimer = Observable
                    .Timer(TimeSpan.FromSeconds(_cooldown.Value))
                    .Subscribe(_ => SpawnNow(spawnRequest));
        }




        private void SpawnNow(SpawnRequest spawnRequest)
        {
            ThrowIfDisposed();

            try
            {
                spawnRequest.SpawnFunction()
                    .Subscribe(obj =>
                    {
                        spawnRequest.Completion.OnNext(obj);
                        spawnRequest.Completion.OnCompleted();
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during spawn: {ex.Message}");
                spawnRequest.Completion.OnErrorResume(ex);
            }
            finally
            {
                _isSpawning.Value = false;
                ProcessQueue();
            }
        }



        protected override void DisposeProtected()
        {
            _currentSpawnTimer?.Dispose();
            _spawnQueue.Clear();
            base.DisposeProtected();
        }



        private class SpawnRequest
        {
            public Func<Observable<Component>> SpawnFunction { get; }
            public Observer<Component> Completion { get; }

            public SpawnRequest(Func<Observable<Component>> spawnFunction, Observer<Component> completion)
            {
                SpawnFunction = spawnFunction;
                Completion = completion;
            }
        }
    }
}