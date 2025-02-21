namespace WhiteArrow.Incremental
{
    public interface IPurchasableLevelsStorage<TSettings> : IPurchasableLevelIconsStorage
        where TSettings : IPurchasableLevel
    {
        int PurchasingIterationsCount { get; }
        int TiersCount { get; }


        int GetMaxLvl(int tier);
        long GetNextLvlPrice(int tier, int lvl);
        TSettings GetLvlSetting(int tier, int lvl);
    }
}