using System;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IRedirectionTarget<TEntityViewModel> : IDisposable
        where TEntityViewModel : IMovableByOneCallProvider
    {
        ReactiveProperty<bool> IsEnabled { get; }

        IReadOnlyObservableList<TEntityViewModel> Entities { get; }

        ReadOnlyReactiveProperty<bool> HasFreeSeat { get; }
        ReadOnlyReactiveProperty<int> SeatsCount { get; }


        Observable<Unit> OnRedirected { get; }
        Observable<Unit> OnArrived { get; }


        bool TryPlace(TEntityViewModel entity);
        bool TryRemoveEntityPeek(out TEntityViewModel entity, bool arrived = true);
        bool TryRemoveEntity(TEntityViewModel entity, bool arrived = true);
    }
}