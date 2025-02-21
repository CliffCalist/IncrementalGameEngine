using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class WeightedRandomVariant<T>
    {
        [SerializeField] private T _item;
        [SerializeField] private int _weight;



        public T Item => _item;
        public int Weight => _weight;



        public WeightedRandomVariant(T item, int weight)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            if (weight < 1)
                throw new ArgumentOutOfRangeException(nameof(weight));
            _weight = weight;
        }

        public WeightedRandomVariant(T item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _weight = 1;
        }
    }
}
