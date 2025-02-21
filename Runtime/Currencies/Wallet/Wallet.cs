using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class Wallet : DisposableBase, IWallet
    {
        private readonly WalletDataAdapter _data;

        public string ResourceName => Resource.ToString();
        public ResourceType Resource => _data.ResourceType;
        public ReadOnlyReactiveProperty<long> Balance => _data.Balance;



        public Wallet(WalletDataAdapter data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            BuildPermanentDisposable(_data);
        }



        public long GetBalance()
        {
            ThrowIfDisposed();
            return _data.Balance.Value;
        }

        public void Deposit(long amount)
        {
            ThrowIfDisposed();

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            _data.Balance.Value += amount;
        }

        public bool TyDebit(long amount)
        {
            ThrowIfDisposed();

            if (amount < 0 || amount > _data.Balance.Value)
                return false;
            else
            {
                _data.Balance.Value -= amount;
                return true;
            }
        }

        public void Reset()
        {
            ThrowIfDisposed();

            if (_data.Balance.Value > 0)
                _data.Balance.Value = 0;
        }
    }
}