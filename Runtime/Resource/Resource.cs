using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class Resource : DisposableBase, IResourceProvider
    {
        private readonly ResourceDataAdapter _data;
        private readonly WalletsStorage _wallet;


        public ResourceType Type => _data.Type;
        public ReadOnlyReactiveProperty<long> Ammount => _data.Ammount;

        Resource IResourceProvider.Resource => this;


        public Resource(ResourceDataAdapter data, WalletsStorage wallet)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

            BuildPermanentDisposable(_data);
        }



        public void Deposit()
        {
            ThrowIfDisposed();

            _wallet.Deposit(Type, Ammount.CurrentValue);
        }
    }
}