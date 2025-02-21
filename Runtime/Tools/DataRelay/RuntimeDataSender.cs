using System;

namespace WhiteArrow.Incremental
{
    public class InlineDataSender<TDataAdapter, TData> : IDataSender<TDataAdapter, TData>, IDisposable
    {
        private TData _data;
        private TDataAdapter _adapter;



        public InlineDataSender(TDataAdapter adapter, TData data)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }



        public TData TakeData(TDataAdapter adapter)
        {
            if (!_adapter.Equals(adapter))
                throw new InvalidOperationException("Adapter mismatch.");
            return _data;
        }

        object IDataSender.TakeData(object adapter)
        {
            if (adapter is TDataAdapter castedAdapter)
                return TakeData(castedAdapter);
            else throw new InvalidCastException(nameof(adapter));
        }



        public void Dispose()
        {
            _data = default;
            _adapter = default;
        }
    }
}