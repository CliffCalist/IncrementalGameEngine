using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class ProcessingService<TProcessableViewModel, TProcessorViewModel, TProcessorFactoryInput> : DisposableBase
        where TProcessableViewModel : IProcessable
        where TProcessorViewModel : class, IProcessor<TProcessableViewModel>
    {
        public readonly ReactiveProperty<bool> IsEnabled = new(true);

        protected readonly EntitiesLifecycle<TProcessorViewModel, TProcessorFactoryInput> _processorsLifecycle;

        protected readonly ObservableList<TProcessableViewModel> _processables = new();
        private readonly Dictionary<TProcessableViewModel, IDisposable> _changeProcessingNeededParameterSubscriptions = new();



        public ProcessingService(EntitiesLifecycle<TProcessorViewModel, TProcessorFactoryInput> processorLifecycle)
        {
            _processorsLifecycle = processorLifecycle ?? throw new ArgumentNullException(nameof(processorLifecycle));



            var processorsAddSubscription = _processorsLifecycle.ViewModels.ObserveAdd()
                .Where(_ => IsEnabled.Value)
                .Subscribe(_ => ProcessPendingPairs());


            var processableAddSubscription = _processables.ObserveAdd()
                .Subscribe(arg =>
                {
                    if (IsEnabled.Value)
                        ProcessPendingPairs();

                    // Subscribe and chaching to change needProcessing state
                    var addedProcessable = arg.Value;
                    var subscription = addedProcessable.IsProcessorRequired
                        .DistinctUntilChanged()
                        .Where(s => s && IsEnabled.CurrentValue)
                        .Subscribe(_ => ProcessPendingPairs());
                    _changeProcessingNeededParameterSubscriptions.Add(addedProcessable, subscription);
                });

            var processableRemoveSubscription = _processables.ObserveRemove()
                .Subscribe(arg =>
                {
                    // Remove chached subscrition to change needProcessing state
                    var removedProcessable = arg.Value;
                    if (_changeProcessingNeededParameterSubscriptions.TryGetValue(removedProcessable, out var subscription))
                    {
                        subscription.Dispose();
                        _changeProcessingNeededParameterSubscriptions.Remove(removedProcessable);
                    }
                });


            var isEnabledSubscription = IsEnabled
                .Skip(1)
                .DistinctUntilChanged()
                .Where(v => v)
                .Subscribe(_ => ProcessPendingPairs());


            BuildPermanentDisposable(
                isEnabledSubscription, processableAddSubscription, processableRemoveSubscription,
                IsEnabled, _processorsLifecycle
            );

            ProcessPendingPairs();
        }



        public virtual void CreateProcessor(TProcessorFactoryInput factoryInput)
        {
            ThrowIfDisposed();

            if (factoryInput is null)
                throw new ArgumentNullException(nameof(factoryInput));

            _processorsLifecycle.Create(factoryInput).Subscribe();
        }

        public void AddProcessable(TProcessableViewModel processable)
        {
            ThrowIfDisposed();

            _processables.Add(processable);
        }


        public void ProcessPendingPairs()
        {
            ThrowIfDisposed();

            while (TryGetNeedProcessProcessable(out var processable) && TryGetFreeProcessor(out var processor))
            {
                var currentProcessable = processable;
                var currentProcessor = processor;

                currentProcessor.IsBusy.ObserveOnMainThread()
                    .Skip(1)
                    .DistinctUntilChanged()
                    .Where(s => !s)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        currentProcessable.OnPostProcessInternal();
                        OnProcessorFreed(currentProcessor);
                        ProcessPendingPairs();
                    });

                currentProcessable.OnPreProcessInternal();
                currentProcessor.StartProcessInternal(processable);
                OnProcessorEngaged(currentProcessor);
            }
        }

        private bool TryGetNeedProcessProcessable(out TProcessableViewModel processable)
        {
            ThrowIfDisposed();

            foreach (var processableObj in _processables)
            {
                if (processableObj.IsProcessorRequired.CurrentValue)
                {
                    processable = processableObj;
                    return true;
                }
            }

            processable = default;
            return false;
        }

        private bool TryGetFreeProcessor(out IProcessor<TProcessableViewModel> processor)
        {
            ThrowIfDisposed();

            foreach (var processorObj in _processorsLifecycle.ViewModels)
            {
                if (!processorObj.IsBusy.CurrentValue)
                {
                    processor = processorObj;
                    return true;
                }
            }

            processor = default;
            return false;
        }

        protected virtual void OnProcessorEngaged(IProcessor<TProcessableViewModel> processor) => ThrowIfDisposed();
        protected virtual void OnProcessorFreed(IProcessor<TProcessableViewModel> processor) => ThrowIfDisposed();




        protected override void DisposeProtected()
        {
            foreach (var kvp in _changeProcessingNeededParameterSubscriptions)
                kvp.Value.Dispose();
            _changeProcessingNeededParameterSubscriptions.Clear();
            _processables.Clear();

            base.DisposeProtected();
        }
    }
}