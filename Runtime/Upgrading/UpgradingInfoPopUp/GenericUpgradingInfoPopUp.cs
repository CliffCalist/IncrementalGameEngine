using System;
using R3;
using TMPro;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class GenericUpgradingInfoPopUp<T> : UpgradingInfoPopUp
    {
        [Space]
        [SerializeField] private TMP_Text _txtSettingsValue;
        [SerializeField] private string _settingsValueFormat = "#";


        private IUpgradingProvider<GenericPurchasableLevel<T>> _currentUpgradingProvider;
        private IDisposable _lvlChangeStream;



        protected override IUpgrading GetUpgrading() => _currentUpgradingProvider.Upgrading;



        public void Bind(IUpgradingProvider<GenericPurchasableLevel<T>> upgradingProvider)
        {
            _currentUpgradingProvider = upgradingProvider ?? throw new ArgumentNullException(nameof(upgradingProvider));
            Rebind();
        }

        protected override void OnRebind()
        {
            base.OnRebind();
            if (_currentUpgradingProvider == null)
                return;

            _lvlChangeStream = _currentUpgradingProvider.Upgrading.Lvl.Value
                .ObserveOnMainThread()
                .Subscribe(lvl =>
                {
                    var settingsValue = _currentUpgradingProvider.Upgrading.CurrentSettings.CurrentValue.Value;
                    _txtSettingsValue.text = _settingsValueFormat.Replace("#", settingsValue.ToString());
                });
        }

        protected override void DisposeBinding()
        {
            base.DisposeBinding();
            _lvlChangeStream?.Dispose();
        }


        protected override void OnValidate()
        {
            base.OnValidate();
            if (!_settingsValueFormat.Contains('#'))
                Debug.LogWarning($"The {nameof(_settingsValueFormat)} field must have '#' symbol. Symbol has be replaced to value from settings.");
        }
    }
}