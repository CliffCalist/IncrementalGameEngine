using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class PositionedProcessingService<TProcessableViewModel, TProcessorViewModel, TProcessorFactoryInput> : ProcessingService<TProcessableViewModel, TProcessorViewModel, TProcessorFactoryInput>
        where TProcessableViewModel : IProcessable
        where TProcessorViewModel : class, IProcessor<TProcessableViewModel>, IMovableByOneCallProvider
    {
        protected readonly Dictionary<Vector3, TProcessorViewModel> _processorsBySeatsMap = new();

        public int SeatsCount => _processorsBySeatsMap.Count;


        public PositionedProcessingService(
            EntitiesLifecycle<TProcessorViewModel, TProcessorFactoryInput> processorLifecycle,
            List<Vector3> seats)
            : base(processorLifecycle)
        {
            if (seats == null)
                throw new ArgumentNullException(nameof(seats));

            for (int i = 0; i < seats.Count; i++)
                _processorsBySeatsMap.Add(seats[i], null);
        }



        public override void CreateProcessor(TProcessorFactoryInput factoryInput)
        {
            ThrowIfDisposed();
            if (TryGetFreeSeat(out var seat))
            {
                _processorsLifecycle.Create(factoryInput)
                    .Subscribe(processor =>
                    {
                        processor.Movement.Wrap(seat);
                        _processorsBySeatsMap[seat] = processor;
                    });
            }
            else throw new InvalidOperationException("Can't find free seat for new processor.");
        }



        protected override void OnProcessorEngaged(IProcessor<TProcessableViewModel> processor)
        {
            ThrowIfDisposed();
            base.OnProcessorEngaged(processor);
            if (TryGetSeatByProcessor(processor as TProcessorViewModel, out var seat))
                _processorsBySeatsMap[seat] = null;
            else throw new InvalidOperationException("Trying clear processor from her seat, but don't finded seat of this processor. It's invalid.");
        }

        protected override void OnProcessorFreed(IProcessor<TProcessableViewModel> processor)
        {
            ThrowIfDisposed();
            base.OnProcessorFreed(processor);
            RedirectProcessorToFreeSeat(processor as TProcessorViewModel);
        }



        private void RedirectProcessorToFreeSeat(TProcessorViewModel processor)
        {
            ThrowIfDisposed();
            if (TryGetFreeSeat(out var seat))
            {
                processor.Movement.SetTarget(seat);
                _processorsBySeatsMap[seat] = processor;
            }
            else throw new InvalidOperationException($"Invoked {nameof(RedirectProcessorToFreeSeat)}, but can't find free seat.");
        }



        protected bool TryGetFreeSeat(out Vector3 seat)
        {
            ThrowIfDisposed();
            foreach (var kvp in _processorsBySeatsMap)
            {
                if (kvp.Value == null)
                {
                    seat = kvp.Key;
                    return true;
                }
            }

            seat = default;
            return false;
        }

        private bool TryGetSeatByProcessor(TProcessorViewModel processor, out Vector3 seat)
        {
            ThrowIfDisposed();
            foreach (var kvp in _processorsBySeatsMap)
            {
                if (kvp.Value == processor)
                {
                    seat = kvp.Key;
                    return true;
                }
            }

            seat = default;
            return false;
        }
    }
}