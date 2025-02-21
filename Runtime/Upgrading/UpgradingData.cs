using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class UpgradingData
    {
        [Min(1)] public int Tier = 1;
        public LevelData Level = new();
        public PurchasingData Purchasing;

        public UpgradingData(ResourceType resourceType)
        {
            Tier = 1;
            Purchasing = new(resourceType);
        }
    }
}