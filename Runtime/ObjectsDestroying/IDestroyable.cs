using R3;

namespace WhiteArrow.Incremental
{
    public interface IDestroyable
    {
        public ReadOnlyReactiveProperty<bool> IsDestroyed { get; }

        public void Destroy();
    }
}
