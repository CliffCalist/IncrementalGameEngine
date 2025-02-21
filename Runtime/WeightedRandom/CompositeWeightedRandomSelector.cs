using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteArrow.Incremental
{
    public class CompositeWeightedRandomSelector : IWeightedRandomSelector
    {
        private readonly List<IWeightedRandomSelector> _selectors = new();

        public int TotalWeight => _selectors.Sum(selector => selector.TotalWeight);



        public CompositeWeightedRandomSelector(List<IWeightedRandomSelector> selectors = null)
        {
            if (selectors != null)
            {
                ValidateSelectors(selectors);
                _selectors = selectors;
            }
        }

        private void ValidateSelectors(List<IWeightedRandomSelector> selectors)
        {
            if (selectors == null || selectors.Count == 0)
                throw new ArgumentException($"{nameof(selectors)} cannot be null or empty.");

            foreach (var selector in selectors)
            {
                if (selector == null || selector.TotalWeight <= 0)
                    throw new ArgumentException("Invalid selector.");
            }
        }



        public void Add(IWeightedRandomSelector selector)
        {
            if (selector == null || selector.TotalWeight <= 0)
                throw new ArgumentException("Invalid selector or weight.");

            _selectors.Add(selector);
        }



        private void PreGetValidation()
        {
            if (_selectors.Count == 0)
                throw new InvalidOperationException("No selectors available for composite spawning.");

        }

        public object GetByWeightAsObject(int weight)
        {
            PreGetValidation();

            int accumulatedWeight = 0;
            foreach (var selectors in _selectors)
            {
                accumulatedWeight += selectors.TotalWeight;
                if (weight < accumulatedWeight)
                    return selectors.GetByWeightAsObject(weight);
            }

            throw new InvalidOperationException("No valid selector was found in the list.");
        }

        public T Get<T>()
        {
            var @object = GetByWeightAsObject(UnityEngine.Random.Range(0, TotalWeight));
            if (@object is T castedObject)
                return castedObject;
            else throw new InvalidCastException($"Geted object is not of type {typeof(T)}.");
        }
    }
}