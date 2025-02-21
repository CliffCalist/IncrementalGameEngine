namespace WhiteArrow.Incremental
{
    public interface IEntityByDataFactoryInput<out TDataAdapter> : IEntityByDataFactoryInput
    {
        new TDataAdapter Adapter { get; }
    }

    public interface IEntityByDataFactoryInput
    {
        object Adapter { get; }
    }
}