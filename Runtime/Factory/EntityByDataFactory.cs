using R3;
using UnityEditor;

namespace WhiteArrow.Incremental
{
    public abstract class EntityByDataFactory<TViewModel> : IEntityFactory<TViewModel, IEntityByDataFactoryInput>
    {
        public abstract Observable<TViewModel> Build(IEntityByDataFactoryInput input);
    }

    public abstract class EntityByDataFactory<TViewModel, TDataAdapter> : EntityByDataFactory<TViewModel>, IEntityFactory<TViewModel, IEntityByDataFactoryInput<TDataAdapter>>
    {
        public abstract Observable<TViewModel> Build(IEntityByDataFactoryInput<TDataAdapter> input);

        public override sealed Observable<TViewModel> Build(IEntityByDataFactoryInput input)
        {
            var castedInput = Tools.SafeCast<IEntityByDataFactoryInput<TDataAdapter>>(input);
            return Build(castedInput).Select(output => output);
        }
    }

    public abstract class EntityByDataFactory<TViewModel, TDataAdapter, TInput> : EntityByDataFactory<TViewModel, TDataAdapter>, IEntityFactory<TViewModel, TInput>
        where TInput : IEntityByDataFactoryInput<TDataAdapter>
    {
        public abstract Observable<TViewModel> Build(TInput input);

        public override sealed Observable<TViewModel> Build(IEntityByDataFactoryInput<TDataAdapter> input)
        {
            var castedInput = Tools.SafeCast<TInput>(input);
            return Build(castedInput).Select(output => output);
        }
    }
}