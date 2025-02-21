using R3;

namespace WhiteArrow.Incremental
{
    public interface ISpawnReceiver<TViewModel>
    {
        ReadOnlyReactiveProperty<int> SlotsCount { get; }
        ReadOnlyReactiveProperty<int> OccupiedSlotsCount { get; }

        Observable<Unit> OnSlotFreeded { get; }


        bool TryReceiveSpawnedEntity(TViewModel viewModel);
    }
}