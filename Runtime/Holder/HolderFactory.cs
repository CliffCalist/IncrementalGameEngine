using System;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public class HolderFactory<TItemViewModel, TItemDataAdapter, TItemData>
        : EntityByDataFactory<Holder<TItemViewModel, TItemDataAdapter, TItemData>, HolderDataAdapter<TItemDataAdapter, TItemData>, HolderFactoryInput<TItemDataAdapter, TItemData>>
        where TItemViewModel : IMovableByOneCallProvider
        where TItemDataAdapter : DataAdapter
    {
        private readonly Func<TItemDataAdapter, Vector3, Observable<TItemViewModel>> _invokeItemFactory;


        public HolderFactory(Func<TItemDataAdapter, Vector3, Observable<TItemViewModel>> invokeItemFactory)
        {
            _invokeItemFactory = invokeItemFactory ?? throw new ArgumentNullException(nameof(invokeItemFactory));
        }


        public override Observable<Holder<TItemViewModel, TItemDataAdapter, TItemData>> Build(
            HolderFactoryInput<TItemDataAdapter, TItemData> input)
        {
            return Observable.Return(
                new Holder<TItemViewModel, TItemDataAdapter, TItemData>(
                    input.Adapter,
                    input.Grid,
                    _invokeItemFactory
                )
            );
        }
    }


    public class HolderFactoryInput<TItemDataAdapter, TItemData> : IEntityByDataFactoryInput<HolderDataAdapter<TItemDataAdapter, TItemData>>
        where TItemDataAdapter : DataAdapter
    {
        public HolderDataAdapter<TItemDataAdapter, TItemData> Adapter { get; }
        object IEntityByDataFactoryInput.Adapter => Adapter;

        public readonly Grid3D Grid;


        public HolderFactoryInput(HolderDataAdapter<TItemDataAdapter, TItemData> adapter, Grid3D grid)
        {
            Adapter = adapter ?? throw new System.ArgumentNullException(nameof(adapter));
            Grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
        }
    }
}