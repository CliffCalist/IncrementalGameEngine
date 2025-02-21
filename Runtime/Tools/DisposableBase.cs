using System;
using System.Linq;
using R3;

namespace WhiteArrow.Incremental
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _isDisposedPrivate;
        protected bool _isDisposed => _isDisposedPrivate;

        private IDisposable _permanentDisposable;



        protected void ThrowIfDisposed()
        {
            if (_isDisposedPrivate)
                throw new ObjectDisposedException(GetType().FullName);
        }



        protected void BuildPermanentDisposable(params IDisposable[] disposables)
        {
            ThrowIfDisposed();

            if (disposables.Contains(null))
                throw new ArgumentNullException();

            var newDisposable = Disposable.Combine(disposables);

            if (_permanentDisposable == null)
                _permanentDisposable = newDisposable;
            else _permanentDisposable = Disposable.Combine(newDisposable, _permanentDisposable);
        }



        public void Dispose()
        {
            if (_isDisposedPrivate)
                return;

            DisposeProtected();
            _isDisposedPrivate = true;
        }

        protected virtual void DisposeProtected()
        {
            DisposePermanentMembers();
        }

        private void DisposePermanentMembers()
        {
            _permanentDisposable?.Dispose();
        }
    }
}