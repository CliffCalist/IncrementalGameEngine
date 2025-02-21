using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class ResourceDataAdapter : DataAdapter
    {
        public ResourceType Type { get; }
        public ReactiveProperty<long> Ammount { get; }


        public ResourceDataAdapter(ResourceData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Type = data.Type;
            Ammount = new(data.Ammount);


            Ammount
                .Skip(1)
                .Subscribe(a => data.Ammount = a);


            BuildPermanentChangesTracker(Ammount.AsUnitObservable().Skip(1));
            BuildPermanentDisposable(Ammount);
        }
    }
}