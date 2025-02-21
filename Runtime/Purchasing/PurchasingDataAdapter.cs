using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class PurchasingDataAdapter : DataAdapter
    {
        public ReactiveProperty<ResourceType> ResourceType { get; }
        public ReactiveProperty<long> InsertedResources { get; }


        public PurchasingDataAdapter(PurchasingData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));


            ResourceType = new(data.ResourceType);
            InsertedResources = new(data.InsertedResources);


            ResourceType
                .Skip(1)
                .Subscribe(t => data.ResourceType = t);

            InsertedResources
                .Skip(1)
                .Subscribe(c => data.InsertedResources = c);


            BuildPermanentChangesTracker(
                ResourceType.Skip(1).AsUnitObservable(),
                InsertedResources.Skip(1).AsUnitObservable()
            );

            BuildPermanentDisposable(ResourceType, InsertedResources);
        }
    }
}