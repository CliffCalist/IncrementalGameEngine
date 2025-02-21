using System;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IProcessor<TProcessable> : IDisposable
        where TProcessable : IProcessable
    {
        protected ReactiveProperty<bool> _isBusy { get; }
        public ReadOnlyReactiveProperty<bool> IsBusy => _isBusy;



        internal void StartProcessInternal(TProcessable processable)
        {
            if (_isBusy.Value)
                throw new Exception($"The method {nameof(StartProcess)} is invoke, but {nameof(IsBusy)} have {_isBusy.CurrentValue} value.");

            _isBusy.Value = true;
            StartProcess(processable);
        }

        protected void StartProcess(TProcessable target);
    }
}