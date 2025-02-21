using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class WalletDataAdapter : DataAdapter
    {
        public readonly ResourceType ResourceType;
        public ReactiveProperty<long> Balance { get; }



        public WalletDataAdapter(WalletData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));


            ResourceType = data.ResourceType;
            Balance = new(data.Balance);


            Balance
                .Skip(1)
                .Subscribe(b => data.Balance = b);


            BuildPermanentChangesTracker(Balance.Skip(1).AsUnitObservable());
            BuildPermanentDisposable(Balance);
        }
    }
}