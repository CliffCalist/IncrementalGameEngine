using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class UpgradingDataAdapter : DataAdapter
    {
        public ReactiveProperty<int> Tier { get; }
        public LevelDataAdapter Level { get; }
        public PurchasingDataAdapter Purchasing { get; }



        public UpgradingDataAdapter(UpgradingData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Tier = new(data.Tier);
            Level = new(data.Level);
            Purchasing = new(data.Purchasing);


            Tier
                .Skip(1)
                .Subscribe(t => data.Tier = t);


            BuildPermanentChangesTracker(
                Tier.Skip(1).AsUnitObservable(),
                Level.OnChanged,
                Purchasing.OnChanged
            );

            BuildPermanentDisposable(Tier, Level, Purchasing);
        }
    }
}