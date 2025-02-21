using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class MonoDataProvider<TValue> : DisposableBase
    {
        private Func<TValue> _getValue;

        private TValue _lastValue;
        public TValue Value
        {
            get
            {
                ThrowIfDisposed();

                try
                {
                    _lastValue = _getValue();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }

                return _lastValue;
            }
        }



        public MonoDataProvider(Func<TValue> getValue)
        {
            ChangeValueSource(getValue);
        }

        public void ChangeValueSource(Func<TValue> getValue)
        {
            ThrowIfDisposed();
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }


        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _getValue = null;
        }
    }
}