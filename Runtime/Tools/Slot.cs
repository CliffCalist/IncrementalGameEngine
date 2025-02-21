using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Slot<TItem> : DisposableBase
    {
        public readonly MonoDataProvider<Vector3> PositionProvider;
        public ReactiveProperty<TItem> Item { get; }


        public ReadOnlyReactiveProperty<bool> IsFree { get; }


        public Slot(MonoDataProvider<Vector3> positionProvider, TItem item = default)
        {
            PositionProvider = positionProvider ?? throw new System.ArgumentNullException(nameof(positionProvider));

            Item = new(item);
            IsFree = Item
                .Select(i => i == null)
                .ToReadOnlyReactiveProperty();

            BuildPermanentDisposable(PositionProvider, Item, IsFree);
        }
    }
}