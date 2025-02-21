// TODO: Complete the transition to the IWallet interface.

using System;
using System.Collections.Generic;

namespace WhiteArrow.Incremental
{
    public class WalletsStorage : DisposableBase, IWalletsStorage
    {
        private readonly WalletsStorageDataAdapter _data;
        private readonly Dictionary<ResourceType, Wallet> _walletsMap = new();



        public WalletsStorage(WalletsStorageDataAdapter data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));

            foreach (var walletData in _data.WalletAdapterMap.Values)
                _walletsMap.Add(walletData.ResourceType, new(walletData));

            BuildPermanentDisposable(_data);
        }



        public long GetBalance(ResourceType resourceType)
        {
            ThrowIfDisposed();
            return GetWallet(resourceType).GetBalance();
        }

        public bool TryDebit(ResourceType resourceType, long value)
        {
            ThrowIfDisposed();
            return GetWallet(resourceType).TyDebit(value);
        }

        public void Deposit(ResourceType resourceType, long value)
        {
            ThrowIfDisposed();
            GetWallet(resourceType).Deposit(value);
        }

        public void Reset(ResourceType resourceType)
        {
            ThrowIfDisposed();
            GetWallet(resourceType).Reset();
        }



        public IWallet GetWallet(ResourceType resourceType)
        {
            ThrowIfDisposed();
            if (_walletsMap.TryGetValue(resourceType, out var wallet))
                return wallet;
            else throw new ArgumentException($"{resourceType} not found");
        }



        protected override void DisposeProtected()
        {
            foreach (var kvp in _walletsMap)
                kvp.Value.Dispose();

            base.DisposeProtected();
        }
    }
}