using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class ForcePuncherDataAdapter : DisposableBase
    {
        public ReactiveProperty<float> Force { get; }
        public ReactiveProperty<float> Angle { get; }



        public ForcePuncherDataAdapter(ForcePuncherData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));


            Force = new(data.Force);
            Angle = new(data.Angle);


            Force
                .Skip(1)
                .Subscribe(f => data.Force = f);

            Angle
                .Skip(1)
                .Subscribe(a => data.Angle = a);


            BuildPermanentDisposable(Force, Angle);
        }
    }
}