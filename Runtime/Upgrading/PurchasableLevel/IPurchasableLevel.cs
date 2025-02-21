namespace WhiteArrow.Incremental
{
    public interface IPurchasableLevel : IPurchasableLevelIcon
    {
        long NextLevelPrice { get; }
    }
}