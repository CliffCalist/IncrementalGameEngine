using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class EntitySpawnerForReceivers<TViewModel> : DisposableBase
    {
        private readonly ObservableHashSet<ISpawnReceiver<TViewModel>> _receivers = new();


        private readonly Func<Observable<TViewModel>> _spawnEntity;
        private readonly Action<TViewModel> _destroyEntity;

        private readonly Dictionary<ISpawnReceiver<TViewModel>, ActionCounter> _spawnCounterMap = new();
        private readonly Dictionary<ISpawnReceiver<TViewModel>, IDisposable> _disposablesMap = new();



        public EntitySpawnerForReceivers(Func<Observable<TViewModel>> spawnEntity, Action<TViewModel> destroyEntity)
        {
            _spawnEntity = spawnEntity ?? throw new ArgumentNullException(nameof(spawnEntity));
            _destroyEntity = destroyEntity ?? throw new ArgumentNullException(nameof(destroyEntity));


            // Observe when reciver are added
            var receiversAddSubscription = _receivers.ObserveAdd()
                .Subscribe(arg =>
                {
                    var receiver = arg.Value;

                    var spawnCounter = new ActionCounter(
                        prop => receiver.SlotsCount.Subscribe(c => prop.Value = c),
                        receiver.OccupiedSlotsCount.CurrentValue
                    );

                    var decrementExecutedSubscription = receiver.OnSlotFreeded
                        .Subscribe(_ => spawnCounter.DecrementExecuted());

                    _disposablesMap.Add(receiver, Disposable.Combine(spawnCounter, decrementExecutedSubscription));
                    _spawnCounterMap.Add(receiver, spawnCounter);

                    spawnCounter.ToPerform
                        .ObserveOnMainThread()
                        .Where(c => c > 0)
                        .Subscribe(_ => RequestSpawn(receiver));
                });

            // Observe when reciver are removed
            var receiversRemoveSubscription = _receivers.ObserveRemove()
                .Subscribe(arg =>
                {
                    var receiver = arg.Value;
                    if (_disposablesMap.ContainsKey(receiver))
                    {
                        _disposablesMap[receiver].Dispose();
                        _disposablesMap.Remove(receiver);
                        _spawnCounterMap.Remove(receiver);
                    }
                });


            BuildPermanentDisposable(receiversAddSubscription, receiversRemoveSubscription);
        }



        public bool HasReceiver(ISpawnReceiver<TViewModel> receiver)
        {
            ThrowIfDisposed();

            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            return _receivers.Contains(receiver);
        }

        public void AddReceiver(ISpawnReceiver<TViewModel> receiver)
        {
            ThrowIfDisposed();

            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            if (!HasReceiver(receiver))
                _receivers.Add(receiver);
        }

        public void RemoveReceiver(ISpawnReceiver<TViewModel> receiver)
        {
            ThrowIfDisposed();

            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            if (HasReceiver(receiver))
                _receivers.Remove(receiver);
        }



        private void RequestSpawn(ISpawnReceiver<TViewModel> receiver)
        {
            ThrowIfDisposed();

            if (!_spawnCounterMap.TryGetValue(receiver, out var spawnCounter))
                return;

            spawnCounter.IncrementPendingActions();
            _spawnEntity()
                .Subscribe(viewModel =>
                    {
                        spawnCounter.ConfirmPendingActionExecution();
                        if (!receiver.TryReceiveSpawnedEntity(viewModel))
                        {
                            Debug.LogError("Failed to receive entity.");
                            _destroyEntity(viewModel);
                            spawnCounter.DecrementExecuted();
                        }
                    });
        }



        protected override void DisposeProtected()
        {
            foreach (var kvp in _spawnCounterMap)
                kvp.Value.Dispose();
            _spawnCounterMap.Clear();

            foreach (var kvp in _disposablesMap)
                kvp.Value.Dispose();
            _disposablesMap.Clear();

            base.DisposeProtected();

            _receivers.Clear();
        }
    }
}