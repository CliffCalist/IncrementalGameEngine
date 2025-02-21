using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class ActionCounter : DisposableBase
    {
        private readonly ReactiveProperty<int> _required = new();
        public ReadOnlyReactiveProperty<int> Required => _required;


        private readonly ReactiveProperty<PendingExecutedPairPrivate> _buffer = new();
        public ReadOnlyReactiveProperty<PendingExecutedPairPrivate> Buffer => _buffer;

        public ReadOnlyReactiveProperty<int> ToPerform { get; }
        public ReadOnlyReactiveProperty<int> Accumulated { get; }




        public ActionCounter(
            Func<ReactiveProperty<int>, IDisposable> requiredPropChangeSubscribing,
            int startExecutedCount)
        {
            if (requiredPropChangeSubscribing is null)
                throw new ArgumentNullException(nameof(requiredPropChangeSubscribing));
            if (startExecutedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(startExecutedCount));


            var requiredSubscription = requiredPropChangeSubscribing(_required);
            _buffer.Value = new(_buffer.CurrentValue.Pending, startExecutedCount);


            ToPerform = _required
                .CombineLatest(
                    _buffer,
                    (required, buffer) => required - buffer.Executed - buffer.Pending)
                .ToReadOnlyReactiveProperty();

            Accumulated = _required
                .CombineLatest(
                    _buffer,
                    (required, buffer) => buffer.Executed + buffer.Pending)
                .ToReadOnlyReactiveProperty();


            BuildPermanentDisposable(requiredSubscription, _required, _buffer, ToPerform, Accumulated);
        }



        /// <summary>
        /// Increments the count of pending actions.
        /// </summary>
        public void IncrementPendingActions()
        {
            ThrowIfDisposed();

            _buffer.Value = new(_buffer.CurrentValue.Pending + 1, _buffer.Value.Executed);
        }

        /// <summary>
        /// Confirms execution of a pending action, reducing pending and adjusting executed buffer.
        /// </summary>
        public void ConfirmPendingActionExecution()
        {
            ThrowIfDisposed();

            if (_buffer.Value.Pending > 0)
                _buffer.Value = new(_buffer.CurrentValue.Pending - 1, _buffer.Value.Executed + 1);
            else throw new InvalidOperationException("No pending actions to confirm.");
        }

        public void DecrementExecuted()
        {
            ThrowIfDisposed();

            _buffer.Value = new(_buffer.CurrentValue.Pending, _buffer.Value.Executed - 1);
        }



        public struct PendingExecutedPairPrivate
        {
            public readonly int Pending;
            public readonly int Executed;


            public PendingExecutedPairPrivate(int pending, int executed)
            {
                Pending = pending;
                Executed = executed;
            }
        }
    }
}