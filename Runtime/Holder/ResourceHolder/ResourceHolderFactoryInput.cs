using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class ResourceHolderFactoryInput<TResourceDataAdapter> : IEntityByDataFactoryInput<TResourceDataAdapter>
    {
        public TResourceDataAdapter Adapter { get; }
        object IEntityByDataFactoryInput.Adapter => Adapter;

        public Vector3? SpawnPoint { get; }


        public ResourceHolderFactoryInput(TResourceDataAdapter adapter, Vector3? spawnPoint = null)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            SpawnPoint = spawnPoint;
        }
    }
}