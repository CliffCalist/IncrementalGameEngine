using R3;

namespace WhiteArrow.Incremental
{
    public interface IProgressProvider
    {
        public ReadOnlyReactiveProperty<float> Progress { get; }
    }
}