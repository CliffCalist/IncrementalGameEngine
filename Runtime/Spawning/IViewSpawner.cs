using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IViewSpawner<T> where T : Component
    {
        void SetMaxSpawnCount(int? newCount);
        Observable<T> Spawn(Transform parent = null, Vector3? position = null, Quaternion? rotation = null);
    }
}