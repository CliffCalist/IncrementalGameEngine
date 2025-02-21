using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteArrow.Incremental
{
    public class WeightedRandomSelector<TItem>
    {
        private readonly List<WeightedRandomVariant<TItem>> _variants = new();

        public int TotalWeight => _variants.Sum(obj => obj.Weight);



        public WeightedRandomSelector(ICollection<WeightedRandomVariant<TItem>> variants)
        {
            if (variants is null)
                throw new ArgumentNullException(nameof(variants));

            _variants = ValidateVariants(variants.ToArray());
        }

        public WeightedRandomSelector(ICollection<TItem> variants)
        {
            if (variants is null)
                throw new ArgumentNullException(nameof(variants));

            var weightedVariants = new WeightedRandomVariant<TItem>[variants.Count];
            var variantIndex = 0;
            foreach (var variant in variants)
            {
                if (variant != null)
                    weightedVariants[variantIndex] = new(variant);
                variantIndex++;
            }
            _variants = ValidateVariants(weightedVariants);
        }

        public WeightedRandomSelector(WeightedRandomVariant<TItem> variant)
        {
            if (variant is null)
                throw new ArgumentNullException(nameof(variant));

            _variants = ValidateVariants(variant);
        }

        public WeightedRandomSelector(TItem variant)
        {
            if (variant is null)
                throw new ArgumentNullException(nameof(variant));

            _variants = ValidateVariants(new WeightedRandomVariant<TItem>(variant));
        }



        private List<WeightedRandomVariant<TItem>> ValidateVariants(params WeightedRandomVariant<TItem>[] variants)
        {
            var validList = new List<WeightedRandomVariant<TItem>>();
            foreach (var obj in variants)
            {
                if (obj.Item != null && obj.Weight > 0)
                    validList.Add(obj);
            }
            return validList;
        }



        public TItem GetByWeight(int weight)
        {
            if (_variants.Count <= 0)
                throw new InvalidOperationException($"The {nameof(_variants)} list is empty.");

            var accumulatedWeight = 0;
            foreach (var variant in _variants)
            {
                accumulatedWeight += variant.Weight;
                if (weight < accumulatedWeight)
                    return variant.Item;
            }

            throw new InvalidOperationException("No valid prefab was found in the list.");
        }

        public TItem Get()
        {
            var weight = UnityEngine.Random.Range(0, TotalWeight);
            return GetByWeight(weight);
        }
    }
}