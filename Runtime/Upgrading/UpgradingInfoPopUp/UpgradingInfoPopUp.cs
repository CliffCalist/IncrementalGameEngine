using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiteArrow.MVVM.UI;

namespace WhiteArrow.Incremental
{
    public abstract class UpgradingInfoPopUp : UiView
    {
        [Space]
        [SerializeField] private Image _imgLevelIcon;

        [Space]
        [SerializeField] private TMP_Text _txtLvl;
        [SerializeField] private string _lvlTextFormat = "LVL #";


        private IDisposable _baseLvlChangeStream;


        protected override void OnRebind()
        {
            var currentUpgradingBase = GetUpgrading();
            if (currentUpgradingBase == null)
                return;

            _baseLvlChangeStream = currentUpgradingBase.Lvl.Value
                .ObserveOnMainThread()
                .Subscribe(lvl =>
                {
                    if (_txtLvl != null)
                        _txtLvl.text = _lvlTextFormat.Replace("#", lvl.ToString());

                    if (_imgLevelIcon != null)
                        _imgLevelIcon.sprite = currentUpgradingBase.CurrentSettings.CurrentValue.LvlIcon;
                });
        }

        protected override void DisposeBinding()
        {
            _baseLvlChangeStream?.Dispose();
        }


        protected abstract IUpgrading GetUpgrading();



        protected virtual void OnValidate()
        {
            if (!_lvlTextFormat.Contains('#'))
                Debug.LogWarning($"The {nameof(_lvlTextFormat)} field must have '#' symbol. Symbol has be replaced to level.");
        }
    }
}