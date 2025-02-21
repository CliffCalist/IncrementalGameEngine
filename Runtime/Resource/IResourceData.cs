namespace WhiteArrow.Incremental
{
    public interface IResourceData
    {
        public ResourceType Type { get; }
        public long Ammount { get; }
    }
}