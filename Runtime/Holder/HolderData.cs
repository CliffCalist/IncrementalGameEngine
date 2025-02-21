using System;
using System.Collections;
using System.Collections.Generic;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class HolderData<TItemData> : IHolderData
    {
        public int MaxCapacity = 100;
        public List<TItemData> Items = new();



        int IHolderData.MaxCapacity
        {
            get => MaxCapacity;
            set => MaxCapacity = value;
        }

        IList IHolderData.Items => Items;



        public HolderData(int maxCapacity)
        {
            if (maxCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            MaxCapacity = maxCapacity;
        }
    }
}