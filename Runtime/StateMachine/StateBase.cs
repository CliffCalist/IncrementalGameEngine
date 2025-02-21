using R3;

namespace WhiteArrow.Incremental
{
    public abstract class StateBase<TContext> : DisposableBase
    {
        protected readonly Subject<Unit> EnteredSubject = new();
        protected readonly Subject<Unit> ExitedSubject = new();

        public Observable<Unit> Entered => EnteredSubject;
        public Observable<Unit> Exited => ExitedSubject;


        public StateBase()
        {
            BuildPermanentDisposable(EnteredSubject, ExitedSubject);
        }


        public virtual void OnEnterState(TContext context)
        {
            ThrowIfDisposed();
            EnteredSubject.OnNext(Unit.Default);
        }

        public virtual void OnExitState(TContext context)
        {
            ThrowIfDisposed();
            ExitedSubject.OnNext(Unit.Default);
        }
    }
}