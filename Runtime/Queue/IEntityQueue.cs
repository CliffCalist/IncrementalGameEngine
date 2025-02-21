using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IEntityQueue<T> where T : IMovableByOneCallProvider
    {
        IReadOnlyObservableList<T> Entities { get; }

        ReadOnlyReactiveProperty<int> SlotsCount { get; }
        ReadOnlyReactiveProperty<int> EnqueuedCount { get; }

        Observable<Unit> OnDequeued { get; }


        bool TryEnqueue(T movementProvider);
    }
}