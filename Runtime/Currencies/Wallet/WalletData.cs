using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class WalletData
    {
        public ResourceType ResourceType;
        public long Balance;


        public WalletData(ResourceType resourceType, long balance = 0)
        {
            ResourceType = resourceType;
            Balance = balance;
        }
    }
}