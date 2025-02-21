using System;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IProcessable
    {
        protected ReactiveProperty<bool> _isProcessorRequired { get; }
        public ReadOnlyReactiveProperty<bool> IsProcessorRequired => _isProcessorRequired;



        internal void OnPreProcessInternal()
        {
            if (!_isProcessorRequired.CurrentValue)
                throw new Exception($"{nameof(OnPreProcess)} is invoke, but processable isn't require processor.");
            _isProcessorRequired.Value = false;
            OnPreProcess();
        }

        void OnPreProcess();



        internal void OnPostProcessInternal()
        {
            _isProcessorRequired.Value = false;
            OnPostProcess();
        }

        void OnPostProcess();
    }
}