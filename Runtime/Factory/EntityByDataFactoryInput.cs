using System;

namespace WhiteArrow.Incremental
{
    public class EntityByDataFactoryInput<TDataAdapter> : IEntityByDataFactoryInput<TDataAdapter>
    {
        public TDataAdapter Adapter { get; }
        object IEntityByDataFactoryInput.Adapter => Adapter;

        public EntityByDataFactoryInput(TDataAdapter adapter)
        {
            if (adapter is null)
                throw new ArgumentOutOfRangeException(nameof(adapter));
            Adapter = adapter;
        }
    }
}