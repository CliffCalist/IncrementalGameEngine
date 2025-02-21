using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IViewAsComponentSpawner : IDisposable
    {
        Observable<Component> SpawnByWeightAsComponent(int weight, Transform parent = null, Vector3? position = null, Quaternion? rotation = null);

        int TotalWeight { get; }
    }
}