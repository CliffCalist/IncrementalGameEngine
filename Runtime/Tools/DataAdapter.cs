using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public abstract class DataAdapter : DisposableBase
    {
        private IDisposable _permanentChangingsDisposable;


        private readonly IDisposable _dynemicChangingsDisposable;
        private readonly ObservableList<Observable<Unit>> _dynemicChangingsObservables = new();
        private readonly Dictionary<object, Observable<Unit>> _originalByUnitChangingsObservables = new();


        private readonly Subject<Unit> _onChanged = new();
        public Observable<Unit> OnChanged => _onChanged;



        public DataAdapter()
        {
            _dynemicChangingsDisposable = _dynemicChangingsObservables
                .ObserveCountChanged()
                .Select(_ => Observable
                    .Merge(_dynemicChangingsObservables)
                    .AsUnitObservable()
                )
                .Switch()
                .Subscribe(_ => _onChanged.OnNext(Unit.Default));


            BuildPermanentDisposable(_dynemicChangingsDisposable);
        }



        protected void BuildPermanentChangesTracker(params Observable<Unit>[] observables)
        {
            ThrowIfDisposed();

            _permanentChangingsDisposable = Observable.Merge(observables)
                .Subscribe(_ => _onChanged.OnNext(Unit.Default));
        }

        protected void AddDynemicChangingsObservable<T>(Observable<T> observable, bool emitOnAdd = true)
        {
            ThrowIfDisposed();

            if (observable is null)
                throw new ArgumentNullException(nameof(observable));


            if (!_originalByUnitChangingsObservables.ContainsKey(observable))
            {
                var unitObservable = observable.AsUnitObservable();
                _dynemicChangingsObservables.Add(unitObservable);
                _originalByUnitChangingsObservables.Add(observable, unitObservable);

                if (emitOnAdd)
                    _onChanged.OnNext(Unit.Default);
            }
        }

        protected void RemoveDynemicChangingsObservable<T>(Observable<T> observable)
        {
            ThrowIfDisposed();

            if (observable is null)
                throw new ArgumentNullException(nameof(observable));


            if (_originalByUnitChangingsObservables.ContainsKey(observable))
            {
                var unitObservable = _originalByUnitChangingsObservables[observable];
                _dynemicChangingsObservables.Remove(unitObservable);
                _originalByUnitChangingsObservables.Remove(observable);
                _onChanged.OnNext(Unit.Default);
            }
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _permanentChangingsDisposable?.Dispose();
        }
    }
}