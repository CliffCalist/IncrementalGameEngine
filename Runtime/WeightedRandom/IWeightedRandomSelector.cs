namespace WhiteArrow.Incremental
{
    public interface IWeightedRandomSelector
    {
        object GetByWeightAsObject(int weight);

        int TotalWeight { get; }
    }
}