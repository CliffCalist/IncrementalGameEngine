using System;

namespace WhiteArrow.Incremental
{
    public class DataRelay<TViewModel, TDataAdapter, TData>
    {
        private readonly IDataSender<TDataAdapter, TData> _sender;
        private readonly TViewModel _viewModel;
        private readonly TDataAdapter _adapter;



        public DataRelay(IDataSender<TDataAdapter, TData> sender, TViewModel viewModel, TDataAdapter adapter)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }


        public void Execute(IDataReceiver<TDataAdapter, TData> receiver, out TViewModel viewModel, out TDataAdapter adapter)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            var data = _sender.TakeData(_adapter);
            receiver.ReceiveData(_adapter, data);

            adapter = _adapter;
            viewModel = _viewModel;
        }
    }
}