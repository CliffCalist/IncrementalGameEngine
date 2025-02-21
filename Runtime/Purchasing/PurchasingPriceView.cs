using System;
using R3;
using TMPro;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class PurchasingPriceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _txt;

        public void Bind(IPurchasingProvider purchasingProvider)
        {
            if (purchasingProvider is null)
                throw new ArgumentNullException(nameof(purchasingProvider));

            purchasingProvider.Purchasing.Price
                .CombineLatest(
                    purchasingProvider.Purchasing.RequiredResources,
                    (price, requared) => (price, requared))
                .ObserveOnMainThread()
                .Subscribe(arg =>
                {
                    if (arg.requared <= 0)
                        _txt.text = arg.price.ToString();
                    else _txt.text = arg.requared.ToString();
                })
                .AddTo(this);
        }
    }
}