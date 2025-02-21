using System;

namespace WhiteArrow.Incremental
{
    public interface IWalletsStorage : IDisposable
    {
        long GetBalance(ResourceType resourceType);
        bool TryDebit(ResourceType resourceType, long value);
        void Deposit(ResourceType resourceType, long value);
        void Reset(ResourceType resourceType);
        IWallet GetWallet(ResourceType resourceType);
    }
}