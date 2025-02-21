using System;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IReactivePropertyUpdater : IDisposable
    {
        ReadOnlyReactiveProperty<bool> IsPaused { get; }


        void ChangeFrameProvider(FrameProvider frameProvider);
        void Stop(bool setDefault);
        void Start();
    }
}