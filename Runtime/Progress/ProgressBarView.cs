using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace WhiteArrow.Incremental
{
    public class ProgressBarView : MonoBehaviour
    {
        [SerializeField] private Image _img;

        public void Bind(IProgressProvider progressProvider)
        {
            if (progressProvider is null)
                throw new ArgumentNullException(nameof(progressProvider));

            progressProvider.Progress
                .ObserveOnMainThread()
                .Subscribe(p => _img.fillAmount = p)
                .AddTo(this);
        }
    }
}