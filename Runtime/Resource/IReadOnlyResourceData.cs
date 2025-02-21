namespace WhiteArrow.Incremental
{
    public interface IReadOnlyResourceData
    {
        public ResourceType Type { get; }
        public long Ammount { get; }
    }
}