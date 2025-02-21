using System;
using R3;

namespace WhiteArrow.Incremental
{
    public interface IEntitiesLifecycle<TViewModel, TFactoryInput> : IDisposable
    {
        Observable<TViewModel> Create(TFactoryInput factoryInput);
        void Destroy(TViewModel viewModel);
    }
}