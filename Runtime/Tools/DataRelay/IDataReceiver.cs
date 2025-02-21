namespace WhiteArrow.Incremental
{
    public interface IDataReceiver
    {
        void ReceiveData(object adapter, object data);
    }

    public interface IDataReceiver<TAdapter, TData> : IDataReceiver
    {
        void ReceiveData(TAdapter adapter, TData data);
    }
}