namespace WhiteArrow.Incremental
{
    public interface IDataSender
    {
        object TakeData(object adapter);
    }

    public interface IDataSender<TDataAdapter, TData> : IDataSender
    {
        TData TakeData(TDataAdapter adapter);
    }
}