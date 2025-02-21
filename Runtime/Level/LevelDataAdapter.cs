using R3;

namespace WhiteArrow.Incremental
{
    public class LevelDataAdapter : DataAdapter
    {
        public readonly ReactiveProperty<int> Lvl;


        public LevelDataAdapter(LevelData data)
        {
            Lvl = new(data.Lvl);

            Lvl
                .Skip(1)
                .Subscribe(l => data.Lvl = l);


            BuildPermanentChangesTracker(Lvl.Skip(1).AsUnitObservable());
            BuildPermanentDisposable(Lvl);
        }
    }
}