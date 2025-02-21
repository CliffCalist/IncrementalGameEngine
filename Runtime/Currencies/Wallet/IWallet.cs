using System;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IWallet : IDisposable
    {
        ResourceType Resource { get; }
        ReadOnlyReactiveProperty<long> Balance { get; }

        long GetBalance();
        bool TyDebit(long amount);
        void Deposit(long amount);
        void Reset();
    }
}