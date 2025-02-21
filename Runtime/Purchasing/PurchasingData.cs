using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class PurchasingData
    {
        public ResourceType ResourceType;
        public long InsertedResources;


        public PurchasingData(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }
    }
}