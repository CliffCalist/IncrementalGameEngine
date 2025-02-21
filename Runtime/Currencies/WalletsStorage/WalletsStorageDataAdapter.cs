using System;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class WalletsStorageDataAdapter : DataAdapter
    {
        private readonly ReactiveMapCollection<WalletDataAdapter, WalletData> _walletDataMap;

        private readonly ObservableDictionary<ResourceType, WalletDataAdapter> _walletAdapterMap = new();
        public IReadOnlyObservableDictionary<ResourceType, WalletDataAdapter> WalletAdapterMap => _walletAdapterMap;



        public WalletsStorageDataAdapter(WalletsStorageData data, WalletSettingsStorage settings)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));


            _walletDataMap = new(d => new(d));
            foreach (var walletData in data.Wallets)
            {
                var adapter = _walletDataMap.Add(walletData);
                _walletAdapterMap.Add(adapter.ResourceType, adapter);
                AddDynemicChangingsObservable(adapter.OnChanged, false);
            }


            var walletDataMapAddSubscription = _walletDataMap.Map.ObserveDictionaryAdd()
                .Subscribe(arg =>
                {
                    data.Wallets.Add(arg.Value);
                    _walletAdapterMap.Add(arg.Value.ResourceType, arg.Key);
                    AddDynemicChangingsObservable(arg.Key.OnChanged);
                });

            var walletDataMapRemoveSubscription = _walletDataMap.Map.ObserveDictionaryRemove()
                .Subscribe(arg =>
                {
                    data.Wallets.Remove(arg.Value);
                    _walletAdapterMap.Remove(arg.Value.ResourceType);
                    RemoveDynemicChangingsObservable(arg.Key.OnChanged);
                });



            BuildPermanentDisposable(
                walletDataMapAddSubscription, walletDataMapRemoveSubscription, _walletDataMap
            );


            // Try find no exist wallet and create it by settings
            foreach (var walletSettings in settings.BaseInstancies)
            {
                if (!WalletAdapterMap.ContainsKey(walletSettings.ResourceType))
                {
                    var walletData = new WalletData(walletSettings.ResourceType, walletSettings.InitBalance);
                    var walletAdapter = _walletDataMap.Add(walletData);
                }
            }
        }



        public WalletDataAdapter Add(WalletData data)
        {
            ThrowIfDisposed();

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            return _walletDataMap.Add(data);
        }

        public void Remove(WalletDataAdapter adapter)
        {
            ThrowIfDisposed();

            if (adapter is null)
                throw new ArgumentNullException(nameof(adapter));

            if (_walletDataMap.ContainsAdapter(adapter))
                _walletDataMap.RemoveByAdapter(adapter);
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _walletAdapterMap.Clear();
        }
    }
}