using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _upgradingParticles;

        public void Bind(ILevelProvider levelProvider)
        {
            if (levelProvider is null)
                throw new ArgumentNullException(nameof(levelProvider));

            levelProvider.Lvl.OnUpgraded
                .ObserveOnMainThread()
                .Subscribe(_ => _upgradingParticles.Play())
                .AddTo(this);

            var chachedObject = gameObject;
            levelProvider.Lvl.IsMaximum
                .ObserveOnMainThread()
                .Subscribe(s => chachedObject.SetActive(!s))
                .AddTo(this);
        }
    }
}
