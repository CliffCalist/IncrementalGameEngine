using R3;

namespace WhiteArrow.Incremental
{
    public interface IEntityFactory<TViewModel, TInput>
    {
        Observable<TViewModel> Build(TInput input);
    }
}