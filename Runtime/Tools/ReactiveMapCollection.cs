using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class ReactiveMapCollection<TAdapter, TData> : DisposableBase
    {
        private readonly ObservableDictionary<TAdapter, TData> _map = new();
        private readonly Func<TData, TAdapter> _adapterFactory;

        public IReadOnlyObservableDictionary<TAdapter, TData> Map => _map;

        private readonly ObservableList<TAdapter> _adapters = new();
        public IReadOnlyObservableList<TAdapter> Adapters => _adapters;

        private readonly ObservableList<TData> _data = new();
        public IReadOnlyObservableList<TData> Data => _data;



        public ReactiveMapCollection(Func<TData, TAdapter> adapterFactory)
        {
            _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));


            var mapAddSubscription = _map.ObserveDictionaryAdd()
                .Subscribe(arg =>
                {
                    _adapters.Add(arg.Key);
                    _data.Add(arg.Value);
                });

            var mapRemoveSubscription = _map.ObserveDictionaryRemove()
                .Subscribe(arg =>
                {
                    _adapters.Remove(arg.Key);
                    _data.Remove(arg.Value);
                });


            BuildPermanentDisposable(mapAddSubscription, mapRemoveSubscription);
        }



        public TAdapter Add(TData data)
        {
            ThrowIfDisposed();

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var adapter = _adapterFactory(data);
            Add(adapter, data);
            return adapter;
        }

        public void Add(TAdapter adapter, TData data)
        {
            ThrowIfDisposed();

            if (adapter is null)
                throw new ArgumentNullException(nameof(adapter));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (_map.ContainsKey(adapter))
                throw new InvalidOperationException("This data is already mapped.");

            _map.Add(adapter, data);
        }



        public TData RemoveByAdapter(TAdapter adapter)
        {
            ThrowIfDisposed();

            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter));

            if (!_map.TryGetValue(adapter, out var data))
                throw new KeyNotFoundException("Adapter not found in the map.");

            _map.Remove(adapter);
            return data;
        }



        public bool ContainsAdapter(TAdapter adapter)
        {
            ThrowIfDisposed();
            return _map.ContainsKey(adapter);
        }

        public bool ContainsData(TData data)
        {
            ThrowIfDisposed();
            return _map.Any(kvp => Object.Equals(kvp.Value, data));
        }

        public void Clear() => _map.Clear();



        protected override void DisposeProtected()
        {
            base.DisposeProtected();

            _adapters.Clear();
            _data.Clear();
            _map.Clear();
        }
    }
}