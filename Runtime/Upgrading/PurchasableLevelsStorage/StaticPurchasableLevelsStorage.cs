using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class StaticPurchasableLevelsStorage<TSettings> : PurchasableLevelsStorage<TSettings>
        where TSettings : IPurchasableLevel
    {
        [SerializeField] private NestedList<TSettings> _tiers;

        public override sealed int TiersCount => _tiers.Count;



        public override sealed int GetMaxLvl(int tier)
        {
            if (tier < 1 || tier > _tiers.Count)
                throw new ArgumentOutOfRangeException(nameof(tier), $"Tier must be in range [1 - {_tiers.Count}], but is {tier}");

            return _tiers[tier - 1].Count;
        }

        protected void ThrowIfInvalidTierOrLvl(int tier, int lvl)
        {
            if (tier < 0 || tier > _tiers.Count)
                throw new ArgumentOutOfRangeException(nameof(tier));
            if (lvl < 0 || lvl > GetMaxLvl(tier))
                throw new ArgumentOutOfRangeException(nameof(lvl));
        }



        public override sealed long GetNextLvlPrice(int tier, int lvl)
        {
            ThrowIfInvalidTierOrLvl(tier, lvl);
            return _tiers[tier - 1][lvl - 1].NextLevelPrice;
        }

        public override sealed Sprite GetLvlIcon(int tier, int lvl)
        {
            ThrowIfInvalidTierOrLvl(tier, lvl);
            return _tiers[tier - 1][lvl - 1].LvlIcon;
        }

        public override sealed TSettings GetLvlSetting(int tier, int lvl)
        {
            ThrowIfInvalidTierOrLvl(tier, lvl);
            return _tiers[tier - 1][lvl - 1];
        }
    }
}