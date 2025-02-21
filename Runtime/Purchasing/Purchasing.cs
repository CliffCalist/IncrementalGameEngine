using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class Purchasing : DisposableBase, IPurchasingProvider, IProgressProvider, IIteratable
    {
        private readonly PurchasingDataAdapter _data;
        private readonly IWalletsStorage _wallet;

        public readonly int IterationsCount;


        public ReactiveProperty<bool> IsEnabled { get; } = new(true);


        private readonly ReactiveProperty<long> _price;
        public ReadOnlyReactiveProperty<long> Price => _price;

        public ReadOnlyReactiveProperty<long> InsertedResources => _data.InsertedResources;
        public ReadOnlyReactiveProperty<long> RequiredResources { get; }
        public ReadOnlyReactiveProperty<long> ResourcesPerIteration { get; }
        public ReadOnlyReactiveProperty<float> Progress { get; }


        private readonly Subject<Unit> _onIterationProcessed = new();
        public Observable<Unit> OnIterrationProcessed => _onIterationProcessed;

        private readonly Subject<Unit> _onPurchased = new();
        public Observable<Unit> OnPurchased => _onPurchased;


        Purchasing IPurchasingProvider.Purchasing => this;


        public Purchasing(PurchasingDataAdapter data, long startPrice, int iterationsCount, IWalletsStorage playerWallet)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _wallet = playerWallet ?? throw new ArgumentNullException(nameof(playerWallet));

            if (startPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(startPrice));
            _price = new(startPrice);

            if (startPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(startPrice));
            IterationsCount = iterationsCount;


            RequiredResources = Price
                .CombineLatest(
                    InsertedResources,
                    (price, inserted) => (price, inserted))
                .Select(arg => Math.Max(arg.price - arg.inserted, 0))
                .ToReadOnlyReactiveProperty();

            ResourcesPerIteration = Price
                .Select(p => Math.Max(p / IterationsCount, 1))
                .ToReadOnlyReactiveProperty();

            Progress = InsertedResources
                .CombineLatest(
                    Price,
                    (inserted, price) => (inserted, price))
                .Select(arg => Mathf.Clamp01((float)arg.inserted / arg.price))
                .ToReadOnlyReactiveProperty();


            BuildPermanentDisposable(
                _data, IsEnabled, _price, RequiredResources, ResourcesPerIteration, Progress,
                _onIterationProcessed, _onPurchased
            );
        }



        public void SetPice(long newPrice)
        {
            ThrowIfDisposed();

            if (newPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(newPrice));

            _price.Value = newPrice;
        }



        public void NextIteration()
        {
            ThrowIfDisposed();

            if (!IsEnabled.Value)
                return;


            var resourceAmmount = _wallet.GetBalance(_data.ResourceType.Value);
            var normalizedInteractionPrice = Math.Min(ResourcesPerIteration.CurrentValue, RequiredResources.CurrentValue);
            normalizedInteractionPrice = Math.Min(normalizedInteractionPrice, resourceAmmount);

            if (normalizedInteractionPrice > 0)
            {
                if (!_wallet.TryDebit(_data.ResourceType.Value, normalizedInteractionPrice))
                    return;

                _data.InsertedResources.Value += normalizedInteractionPrice;
                if (InsertedResources.CurrentValue >= Price.CurrentValue)
                {
                    var newInsertedValue = InsertedResources.CurrentValue - Price.CurrentValue;
                    _data.InsertedResources.Value = Math.Max(newInsertedValue, 0);
                    _onPurchased.OnNext(Unit.Default);
                }

                _onIterationProcessed.OnNext(Unit.Default);
            }
        }
    }
}