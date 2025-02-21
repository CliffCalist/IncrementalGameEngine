using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using R3.Triggers;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Magnet<TItem> : DisposableBase
        where TItem : IMovableByOneCallProvider, IDestroyable
    {
        private readonly ObservableList<TItem> _triggeredItems = new();
        private readonly Dictionary<TItem, IDisposable> _itemIsArrivedSubscriptionMap = new();
        private readonly Action<TItem> _reciveItem;


        private readonly ReadOnlyReactiveProperty<Vector3> _positionProvider;


        public bool DestroyItemAfterArriving = true;
        public bool UseDynamicMovementTarget = true;



        public Magnet(
            ReadOnlyReactiveProperty<Vector3> positionProvider,
            Component source,
            Action<TItem> reciveItem)
        {
            _positionProvider = positionProvider ?? throw new ArgumentNullException(nameof(positionProvider));
            _reciveItem = reciveItem ?? throw new ArgumentNullException(nameof(reciveItem));


            var triggerSubscription = source.OnTriggerEnterAsObservable()
                .Subscribe(c =>
                {
                    if (!c.TryGetComponent(out NonMonoDataProvider<TItem> itemProvider))
                        return;

                    if (itemProvider.HasValue && !_triggeredItems.Contains(itemProvider.Value))
                        _triggeredItems.Add(itemProvider.Value);
                });

            var triggeredItemsSubceription = _triggeredItems
                .ObserveAdd()
                .Subscribe(e => OnNewItemAdded(e.Value));


            BuildPermanentDisposable(
                triggerSubscription, triggeredItemsSubceription, _positionProvider
            );
        }



        private void OnNewItemAdded(TItem item)
        {
            ThrowIfDisposed();

            if (UseDynamicMovementTarget)
                item.Movement.SetDynamicTarget(_positionProvider, true);
            else item.Movement.SetTarget(_positionProvider.CurrentValue);

            var isArrivedStream = item.Movement.IsArrived
                .Where(i => i)
                .Take(1)
                .Subscribe(_ => OnItemArrived(item));

            _itemIsArrivedSubscriptionMap.Add(item, isArrivedStream);
        }

        private void OnItemArrived(TItem item)
        {
            ThrowIfDisposed();

            if (_triggeredItems.Contains(item))
                _triggeredItems.Remove(item);

            if (_itemIsArrivedSubscriptionMap.TryGetValue(item, out var subscription))
            {
                subscription.Dispose();
                _itemIsArrivedSubscriptionMap.Remove(item);
            }

            _reciveItem(item);

            if (DestroyItemAfterArriving)
                item.Destroy();
        }



        protected override void DisposeProtected()
        {
            foreach (var stream in _itemIsArrivedSubscriptionMap.Values)
                stream.Dispose();
            _itemIsArrivedSubscriptionMap.Clear();

            base.DisposeProtected();
        }
    }
}