using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public abstract class PurchasableLevel : IPurchasableLevel
    {
        [Min(0)] public long LevelPrice = 1;
        long IPurchasableLevel.NextLevelPrice => LevelPrice;


        [SerializeField] private Sprite _icon;
        public Sprite LvlIcon => _icon;
    }
}