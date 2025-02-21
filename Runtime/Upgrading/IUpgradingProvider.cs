namespace WhiteArrow.Incremental
{
    public interface IUpgradingProvider<TSettings> where TSettings : IPurchasableLevel
    {
        Upgrading<TSettings> Upgrading { get; }
    }
}