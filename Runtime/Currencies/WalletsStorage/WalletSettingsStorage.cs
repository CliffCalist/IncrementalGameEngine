using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class WalletSettingsStorage : ScriptableObject
    {
        public abstract IReadOnlyList<WalletInstanceSettings> BaseInstancies { get; }
    }

    public class WalletSettingsStorage<T> : WalletSettingsStorage
        where T : WalletInstanceSettings
    {
        [SerializeField] private List<T> _wallets;


        public override sealed IReadOnlyList<WalletInstanceSettings> BaseInstancies => Instancies;

        public IReadOnlyList<T> Instancies
        {
            get
            {
                var uniqueInstances = new Dictionary<ResourceType, T>();
                foreach (var instance in _wallets)
                {
                    if (uniqueInstances.ContainsKey(instance.ResourceType))
                        Debug.LogWarning($"Duplicate ResourceType detected: {instance.ResourceType}. Duplicate instance removed.");
                    else
                        uniqueInstances.Add(instance.ResourceType, instance);
                }
                return uniqueInstances.Values.ToList();
            }
        }
    }
}