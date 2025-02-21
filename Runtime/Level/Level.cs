using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class Level : DisposableBase, ILevelProvider
    {
        private readonly LevelDataAdapter _data;


        private readonly ReactiveProperty<int> _maxLvl;
        public ReadOnlyReactiveProperty<int> MaxLvl => _maxLvl;

        public ReadOnlyReactiveProperty<int> Value => _data.Lvl;
        public ReadOnlyReactiveProperty<bool> IsMaximum { get; }


        private readonly Subject<int> _onUpgraded = new();
        private readonly Subject<int> _onReseted = new();

        public Observable<int> OnUpgraded => _onUpgraded;
        public Observable<int> OnReseted => _onReseted;

        Level ILevelProvider.Lvl => this;

        public const int DEFAULT_MINIMUM_LVL_VALUE = 1;



        public Level(LevelDataAdapter data, int maxLvl)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            if (maxLvl <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLvl));


            _maxLvl = new(maxLvl);

            IsMaximum = MaxLvl
                .CombineLatest(
                    Value,
                    (max, lvl) => max == -1 ? false : lvl >= max)
                .ToReadOnlyReactiveProperty();


            BuildPermanentDisposable(IsMaximum, _data, _maxLvl, _onUpgraded, _onReseted);
        }



        public void SetMax(int max)
        {
            ThrowIfDisposed();

            if (max <= 0)
                throw new ArgumentOutOfRangeException(nameof(max));

            _maxLvl.Value = max;
        }



        public void Upgrade()
        {
            ThrowIfDisposed();

            if (IsMaximum.CurrentValue)
                throw new InvalidOperationException("Can't upgrade level because it is maximum.");

            _data.Lvl.Value++;
            _onUpgraded.OnNext(Value.CurrentValue);
        }

        public void Reset()
        {
            ThrowIfDisposed();

            _data.Lvl.Value = DEFAULT_MINIMUM_LVL_VALUE;
            _onReseted.OnNext(Value.CurrentValue);
        }
    }
}