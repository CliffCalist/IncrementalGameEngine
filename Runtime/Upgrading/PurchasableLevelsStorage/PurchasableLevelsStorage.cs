using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class PurchasableLevelsStorage<TSettings> : ScriptableObject, IPurchasableLevelsStorage<TSettings>
        where TSettings : IPurchasableLevel
    {
        [Header("Upgrading")]
        [SerializeField, Min(1)] protected int _purchasingIterationCount = 8;


        public int PurchasingIterationsCount => _purchasingIterationCount;
        public abstract int TiersCount { get; }


        public abstract int GetMaxLvl(int tier);
        public abstract long GetNextLvlPrice(int tier, int lvl);
        public abstract Sprite GetLvlIcon(int tier, int lvl);
        public abstract TSettings GetLvlSetting(int tier, int lvl);
    }
}