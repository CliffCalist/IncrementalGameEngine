using R3;

namespace WhiteArrow.Incremental
{
    public interface IUpgrading : ILevelProvider, IProgressProvider
    {
        Purchasing Purchasing { get; }
        ReadOnlyReactiveProperty<int> Tier { get; }

        ReadOnlyReactiveProperty<IPurchasableLevel> CurrentSettings { get; }


        void ResetLvl();
        void ResetLvl(int newTier);
    }
}