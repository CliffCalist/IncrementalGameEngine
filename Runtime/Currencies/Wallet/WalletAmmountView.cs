using System;
using R3;
using TMPro;
using UnityEngine;
using WhiteArrow.MVVM.UI;

namespace WhiteArrow.Incremental
{
    public class WalletAmmountView : UiView
    {
        [SerializeField] protected TMP_Text _txt;


        private IWallet _wallet;
        private IDisposable _bindingDisposable;


        public void Bind(IWallet wallet)
        {
            if (wallet is null)
                throw new ArgumentNullException(nameof(wallet));

            _wallet = wallet;
            Rebind();
        }

        protected override void OnRebind()
        {
            if (_wallet == null)
                return;

            _bindingDisposable = _wallet.Balance
                .ObserveOnMainThread()
                .Subscribe(b => _txt.text = b.ToString());
        }

        protected override void DisposeBinding()
        {
            _bindingDisposable?.Dispose();
        }
    }
}