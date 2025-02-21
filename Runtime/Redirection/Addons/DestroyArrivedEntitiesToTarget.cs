using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class DestroyArrivedEntitiesToTarget<T> : DisposableBase
        where T : IMovableByOneCallProvider, IDestroyable
    {


        public DestroyArrivedEntitiesToTarget(IRedirectionTarget<T> redirectionTarget)
        {
            if (redirectionTarget is null)
                throw new ArgumentNullException(nameof(redirectionTarget));

            var onArrivedSubscription = redirectionTarget.OnArrived
                .Subscribe(_ =>
                {
                    while (redirectionTarget.TryRemoveEntityPeek(out var entity))
                        Destroying.Destr(entity);
                });


            BuildPermanentDisposable(onArrivedSubscription);
        }
    }
}