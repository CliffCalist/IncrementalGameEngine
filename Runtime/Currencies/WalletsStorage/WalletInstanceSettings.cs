using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class WalletInstanceSettings
    {
        [SerializeField] private ResourceType _resourceType;
        public ResourceType ResourceType => _resourceType;


        [SerializeField, Min(0)] private long _initBalance;
        public long InitBalance => _initBalance;




        public WalletInstanceSettings(ResourceType resourceType, long initBalance)
        {
            _resourceType = resourceType;
            _initBalance = initBalance;
        }
    }
}