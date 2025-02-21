using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Upgrading<TSettings> : DisposableBase, IUpgrading, IUpgradingProvider<TSettings>, ILevelProvider, IProgressProvider
        where TSettings : IPurchasableLevel
    {
        private readonly UpgradingDataAdapter _data;
        private readonly IPurchasableLevelsStorage<TSettings> _levelsStorage;


        public Purchasing Purchasing { get; }
        public IteratorZone Iterator;


        public ReadOnlyReactiveProperty<int> Tier => _data.Tier;

        public Level Lvl { get; }


        private readonly ReactiveProperty<TSettings> _currentSettings;
        public ReadOnlyReactiveProperty<TSettings> CurrentSettings => _currentSettings;
        ReadOnlyReactiveProperty<IPurchasableLevel> IUpgrading.CurrentSettings => _currentSettings.Select(s => (IPurchasableLevel)s).ToReadOnlyReactiveProperty();


        public ReadOnlyReactiveProperty<float> Progress => Purchasing.Progress;


        Upgrading<TSettings> IUpgradingProvider<TSettings>.Upgrading => this;



        public Upgrading(
            UpgradingDataAdapter data,
            IPurchasableLevelsStorage<TSettings> levelsStorage,
            IteratorZoneSettings iteratorSettings,
            Collider iteratorSourse,
            IWalletsStorage playerWallet)
        {
            if (iteratorSettings is null)
                throw new ArgumentNullException(nameof(iteratorSettings));

            if (iteratorSourse is null)
                throw new ArgumentNullException(nameof(iteratorSourse));

            if (playerWallet is null)
                throw new ArgumentNullException(nameof(playerWallet));



            _data = data ?? throw new ArgumentNullException(nameof(data));
            _levelsStorage = levelsStorage ?? throw new ArgumentNullException(nameof(levelsStorage));

            _data.Tier.Value = Mathf.Min(_levelsStorage.TiersCount, _data.Tier.Value);
            Lvl = new(data.Level, _levelsStorage.GetMaxLvl(_data.Tier.Value));

            Purchasing = new(
                data.Purchasing,
                _levelsStorage.GetNextLvlPrice(_data.Tier.Value, Lvl.Value.CurrentValue),
                _levelsStorage.PurchasingIterationsCount,
                playerWallet);

            Iterator = new(iteratorSourse, Purchasing, iteratorSettings);
            _currentSettings = new(_levelsStorage.GetLvlSetting(Tier.CurrentValue, Lvl.Value.CurrentValue));



            Purchasing.OnPurchased
                .Subscribe(_ =>
                {
                    if (Lvl.IsMaximum.CurrentValue)
                    {
                        Debug.LogWarning($"{nameof(Level)} in {nameof(Upgrading<TSettings>)} is maximum, but has been purchased level-up.");
                        return;
                    }
                    Lvl.Upgrade();
                });



            Lvl.Value
                .Subscribe(lvl =>
                {
                    _currentSettings.Value = _levelsStorage.GetLvlSetting(Tier.CurrentValue, lvl);
                    Purchasing.SetPice(_levelsStorage.GetNextLvlPrice(Tier.CurrentValue, lvl));
                });

            Lvl.IsMaximum
                .Subscribe(isMaximum => Purchasing.IsEnabled.Value = !isMaximum);



            BuildPermanentDisposable(Iterator, Purchasing, Lvl, _data, _currentSettings);
        }



        public void ResetLvl()
        {
            ThrowIfDisposed();
            Lvl.Reset();
        }

        public void ResetLvl(int newTier)
        {
            ThrowIfDisposed();

            _data.Tier.Value = Mathf.Min(_levelsStorage.TiersCount, newTier);
            Lvl.Reset();
        }
    }
}