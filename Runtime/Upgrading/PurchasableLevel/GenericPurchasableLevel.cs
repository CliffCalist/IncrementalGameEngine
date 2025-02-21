using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class GenericPurchasableLevel<T> : IGenericPurchasableLevel
    {
        [Min(0)] public long NextLevelPrice = 1;
        long IPurchasableLevel.NextLevelPrice => NextLevelPrice;


        [SerializeField] private Sprite Icon;
        public Sprite LvlIcon => Icon;


        public T Value;
        object IGenericPurchasableLevel.Value => Value;
    }
}