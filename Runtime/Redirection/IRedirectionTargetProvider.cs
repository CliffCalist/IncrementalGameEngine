namespace WhiteArrow.Incremental
{
    public interface IRedirectionTargetProvider<TEntityViewModel>
        where TEntityViewModel : IMovableByOneCallProvider
    {
        IRedirectionTarget<TEntityViewModel> RedirectionTarget { get; }
    }
}