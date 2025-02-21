using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Holder<TItemViewModel, TItemDataAdapter, TItemData> : DisposableBase
        where TItemViewModel : IMovableByOneCallProvider
        where TItemDataAdapter : DataAdapter
    {
        private readonly HolderDataAdapter<TItemDataAdapter, TItemData> _data;
        private readonly ObservableDictionary<TItemViewModel, TItemDataAdapter> _itemsDataMap = new();


        private readonly ObservableDictionary<TItemViewModel, Vector3Int> _seatsMap = new();
        private readonly Grid3D _gridCore;
        private readonly TItemViewModel[,,] _grid;


        private readonly Dictionary<TItemViewModel, IDisposable> _destroySubscriptions = new();



        public ReadOnlyReactiveProperty<int> MaxCapacity => _data.MaxCapacity;
        public ReadOnlyReactiveProperty<int> ItemsCount => _data.ItemsCount;


        public ReadOnlyReactiveProperty<bool> CanHold { get; }
        public ReadOnlyReactiveProperty<bool> CanTake { get; }


        public Vector3 GridSize => _gridCore.ScaledWorldSize;



        public Holder(
            HolderDataAdapter<TItemDataAdapter, TItemData> data,
            Grid3D gridCore,
            Func<TItemDataAdapter, Vector3, Observable<TItemViewModel>> invokeItemFactory)
        {
            if (invokeItemFactory == null)
                throw new ArgumentNullException(nameof(invokeItemFactory));


            _data = data ?? throw new ArgumentNullException(nameof(data));

            _gridCore = gridCore ?? throw new ArgumentNullException(nameof(gridCore));
            _grid = new TItemViewModel[_gridCore.Size.x, _gridCore.Size.y, _gridCore.Size.z];


            foreach (var itemAdapter in _data.Items)
            {
                var freeSeat = GetFirstFreeSeat();
                if (!freeSeat.HasValue)
                    throw new InvalidOperationException("Cannot find free seat for item from data.");

                var spawnPosition = _gridCore.GetWorldPosition(freeSeat.Value);

                invokeItemFactory(itemAdapter, spawnPosition)
                    .Subscribe(viewModel =>
                    {
                        _itemsDataMap.Add(viewModel, itemAdapter);
                        _seatsMap.Add(viewModel, freeSeat.Value);

                        var seat = freeSeat.Value;
                        _grid[seat.x, seat.y, seat.z] = viewModel;


                        if (viewModel is IDestroyable destroyable)
                        {
                            var subscription = destroyable.IsDestroyed
                                .Where(s => s)
                                .Subscribe(_ =>
                                {
                                    _itemsDataMap.Remove(viewModel);
                                    _data.TryRemoveItem(itemAdapter, out var _);
                                });

                            _destroySubscriptions.Add(viewModel, subscription);
                        }
                    });
            }



            CanHold = ItemsCount
                .DistinctUntilChanged()
                .Select(c => c < MaxCapacity.CurrentValue)
                .ToReadOnlyReactiveProperty();

            CanTake = ItemsCount
                .DistinctUntilChanged()
                .Select(c => c > 0)
                .ToReadOnlyReactiveProperty();



            var seatsMapAddStream = _seatsMap.ObserveDictionaryAdd()
                .ObserveOnMainThread()
                .Subscribe(arg =>
                {
                    var item = arg.Key;
                    var seat = arg.Value;
                    _grid[seat.x, seat.y, seat.z] = item;

                    var seatPosition = _gridCore.GetWorldPosition(seat);
                    item.Movement.SetTarget(seatPosition);
                });

            var seatsMapRemoveStream = _seatsMap.ObserveDictionaryRemove()
                .Subscribe(arg =>
                {
                    var seat = arg.Value;
                    _grid[seat.x, seat.y, seat.z] = default;

                    var item = arg.Key;
                    if (_destroySubscriptions.ContainsKey(item))
                    {
                        _destroySubscriptions[item].Dispose();
                        _destroySubscriptions.Remove(item);
                    }
                });



            var itemsDataMapAddStream = _itemsDataMap.ObserveDictionaryAdd()
                .Subscribe(arg =>
                {
                    var freeSeat = GetFirstFreeSeat();
                    if (!freeSeat.HasValue)
                        throw new InvalidOperationException($"Cannot find free seat for new item {arg.Value}.");

                    _seatsMap.Add(arg.Key, freeSeat.Value);


                    if (arg.Key is IDestroyable destroyable)
                    {
                        var subscription = destroyable.IsDestroyed
                            .Where(s => s)
                            .Subscribe(_ =>
                            {
                                _itemsDataMap.Remove(arg.Key);
                                _data.TryRemoveItem(arg.Value, out var _);
                            });

                        _destroySubscriptions.Add(arg.Key, subscription);
                    }
                });

            var itemsDataMapRemoveStream = _itemsDataMap.ObserveDictionaryRemove()
                .Subscribe(arg => _seatsMap.Remove(arg.Key));



            BuildPermanentDisposable(
                seatsMapAddStream, seatsMapRemoveStream, itemsDataMapRemoveStream, itemsDataMapAddStream,
                CanHold, CanTake, _data
            );
        }



        public void ChangeGridOffset(Vector3 newOffset)
        {
            ThrowIfDisposed();

            _gridCore.Offset = newOffset;
            UdpateItemsPositions();
        }

        public void UdpateItemsPositions()
        {
            ThrowIfDisposed();

            foreach (var kvp in _seatsMap)
            {
                var position = _gridCore.GetWorldPosition(kvp.Value);
                kvp.Key.Movement.Wrap(position);
            }
        }



        public bool TryHold(DataRelay<TItemViewModel, TItemDataAdapter, TItemData> dataRelay)
        {
            ThrowIfDisposed();

            if (dataRelay is null || !CanHold.CurrentValue)
                return false;

            dataRelay.Execute(_data, out var viewModel, out var adapter);
            _itemsDataMap.Add(viewModel, adapter);
            return true;
        }


        public bool TryTake(out DataRelay<TItemViewModel, TItemDataAdapter, TItemData> dataRelay)
        {
            ThrowIfDisposed();

            if (CanTake.CurrentValue)
            {
                var lastNonFreeSeat = GetLastNonFreeSeat();
                if (!lastNonFreeSeat.HasValue)
                {
                    dataRelay = default;
                    return false;
                }

                var itemViewModel = _grid[lastNonFreeSeat.Value.x, lastNonFreeSeat.Value.y, lastNonFreeSeat.Value.z];
                if (itemViewModel == null || !_itemsDataMap.ContainsKey(itemViewModel))
                {
                    Debug.LogWarning("Grid and items mapping desynchronized. Correcting the state.");
                    _grid[lastNonFreeSeat.Value.x, lastNonFreeSeat.Value.y, lastNonFreeSeat.Value.z] = default;
                    dataRelay = default;
                    return false;
                }

                var data = _itemsDataMap[itemViewModel];
                dataRelay = new(_data, itemViewModel, data);
                _itemsDataMap.Remove(itemViewModel);
                return true;
            }

            dataRelay = default;
            return false;
        }



        private Vector3Int? GetFirstFreeSeat()
        {
            ThrowIfDisposed();

            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                for (int x = 0; x < _grid.GetLength(0); x++)
                {
                    for (int z = 0; z < _grid.GetLength(2); z++)
                    {
                        if (_grid[x, y, z] is null)
                            return new(x, y, z);
                    }
                }
            }

            return null;
        }

        private Vector3Int? GetLastNonFreeSeat()
        {
            ThrowIfDisposed();

            for (int y = _grid.GetLength(1) - 1; y >= 0; y--)
            {
                for (int x = _grid.GetLength(0) - 1; x >= 0; x--)
                {
                    for (int z = _grid.GetLength(2) - 1; z >= 0; z--)
                    {
                        if (_grid[z, y, z] is not null)
                            return new(x, y, z);
                    }
                }
            }

            return null;
        }



        protected override void DisposeProtected()
        {
            foreach (var kvp in _destroySubscriptions)
                kvp.Value.Dispose();
            _destroySubscriptions.Clear();

            base.DisposeProtected();

            foreach (var kvp in _itemsDataMap)
            {
                if (kvp.Key is IDisposable disposableKey)
                    disposableKey.Dispose();

                if (kvp.Value is IDisposable disposableValue)
                    disposableValue.Dispose();
            }
            _itemsDataMap.Clear();
        }



#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (!_isDisposed)
                _gridCore?.OnDrawGizmos();
        }
#endif
    }
}