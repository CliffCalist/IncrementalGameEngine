using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public enum ActivationByLvl { ActivateUpToLvl, ActivateOnlyLvl }

    public class ObjectsActivatorByLevelView : MonoBehaviour
    {
        [SerializeField] private ActivationByLvl _mode = ActivationByLvl.ActivateUpToLvl;
        [SerializeField] private List<ActivationLevel> _levels;


        public void Bind(ILevelProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            provider.Lvl.Value
                .ObserveOnMainThread()
                .Subscribe(l =>
                {
                    for (int i = 1; i <= _levels.Count; i++)
                    {
                        var isActive = IsActive(i, l);

                        var level = _levels[i - 1];
                        level.ActivateObjects.ForEach(e => e?.SetActive(isActive));
                        level.DeactivateObjects.ForEach(e => e?.SetActive(!isActive));
                    }
                })
                .AddTo(this);
        }


        private bool IsActive(int activationLvl, int lvl) => _mode switch
        {
            ActivationByLvl.ActivateUpToLvl => activationLvl <= lvl,
            ActivationByLvl.ActivateOnlyLvl => activationLvl == lvl,
            _ => false
        };
    }
}