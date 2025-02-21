using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public abstract class NonMonoDataProvider<TValue> : MonoBehaviour
    {
        public TValue Value { get; private set; }

        public bool HasValue => Value != null;


        public void Set(TValue item)
        {
            Value = item ?? throw new ArgumentNullException(nameof(item));
        }
    }
}