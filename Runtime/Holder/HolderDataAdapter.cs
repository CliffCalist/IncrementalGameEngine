using System;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class HolderDataAdapter<TItemDataAdapter, TItemData> : DataAdapter, IDataReceiver<TItemDataAdapter, TItemData>, IDataSender<TItemDataAdapter, TItemData>
        where TItemDataAdapter : DataAdapter
    {
        private readonly ReactiveMapCollection<TItemDataAdapter, TItemData> _itemDataMap;
        public IReadOnlyObservableList<TItemDataAdapter> Items => _itemDataMap.Adapters;

        public ReactiveProperty<int> MaxCapacity { get; }
        public ReadOnlyReactiveProperty<int> ItemsCount { get; }



        public HolderDataAdapter(
            HolderData<TItemData> data,
            Func<TItemData, TItemDataAdapter> itemAdapterFactory)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (itemAdapterFactory is null)
                throw new ArgumentNullException(nameof(itemAdapterFactory));


            MaxCapacity = new(data.MaxCapacity);


            _itemDataMap = new(itemAdapterFactory);
            data.Items.ForEach(itemData =>
            {
                var adapter = _itemDataMap.Add(itemData);
                AddDynemicChangingsObservable(adapter.OnChanged, false);
            });


            var itemsMapAddSubscription = _itemDataMap.Map
                .ObserveDictionaryAdd()
                .Subscribe(arg =>
                {
                    data.Items.Add(arg.Value);
                    AddDynemicChangingsObservable(arg.Key.OnChanged);
                });

            var itemsMapRemoveSubscription = _itemDataMap.Map
                .ObserveDictionaryRemove()
                .Subscribe(arg =>
                {
                    data.Items.Remove(arg.Value);
                    RemoveDynemicChangingsObservable(arg.Key.OnChanged);
                });


            ItemsCount = _itemDataMap.Adapters
                .ObserveCountChanged()
                .ToReadOnlyReactiveProperty();

            MaxCapacity
                .Skip(1)
                .Subscribe(c => data.MaxCapacity = c);



            BuildPermanentChangesTracker(
                MaxCapacity.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(
                MaxCapacity, ItemsCount, itemsMapAddSubscription, itemsMapRemoveSubscription, _itemDataMap
            );
        }



        public void ReceiveData(TItemDataAdapter adapter, TItemData data)
        {
            ThrowIfDisposed();

            if (adapter is null)
                throw new ArgumentNullException(nameof(adapter));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (MaxCapacity.CurrentValue <= ItemsCount.CurrentValue)
                throw new InvalidOperationException($"{nameof(MaxCapacity)}({MaxCapacity.CurrentValue}) of Holder is reached.");

            _itemDataMap.Add(adapter, data);
        }

        void IDataReceiver.ReceiveData(object adapter, object data)
        {
            ThrowIfDisposed();


            if (adapter is TItemDataAdapter castedAdapter)
            {
                if (data is TItemData castedData)
                    ReceiveData(castedAdapter, castedData);
                else throw new InvalidCastException(nameof(data));
            }
            else throw new InvalidCastException(nameof(adapter));
        }



        public TItemData TakeData(TItemDataAdapter adapter)
        {
            ThrowIfDisposed();

            if (adapter is null)
                throw new ArgumentNullException(nameof(adapter));


            if (_itemDataMap.Map.TryGetValue(adapter, out var data))
            {
                _itemDataMap.RemoveByAdapter(adapter);
                return data;
            }
            else throw new ArgumentOutOfRangeException($"This {typeof(TItemDataAdapter).Name} isn't contains in this {nameof(HolderDataAdapter<TItemDataAdapter, TItemData>)}");
        }

        object IDataSender.TakeData(object adapter)
        {
            ThrowIfDisposed();

            if (adapter is TItemDataAdapter castedAdapter)
                return TakeData(castedAdapter);
            else throw new InvalidCastException(nameof(adapter));
        }


        public bool TryRemoveItem(TItemDataAdapter adapter, out TItemData data)
        {
            ThrowIfDisposed();

            if (_itemDataMap.Map.TryGetValue(adapter, out data))
            {
                _itemDataMap.RemoveByAdapter(adapter);
                return true;
            }
            else return false;
        }
    }
}