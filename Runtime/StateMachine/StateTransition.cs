using System;
using R3;

namespace WhiteArrow.Incremental
{
    public class StateTransition<TStateID> : DisposableBase
        where TStateID : Enum
    {
        public TStateID From { get; }
        public TStateID To { get; }

        private readonly Subject<Unit> _condition = new();
        public Observable<Unit> Condition => _condition;



        public StateTransition(TStateID from, TStateID to, Observable<Unit> condition)
        {
            From = from;
            To = to;

            var conditionSubscription = condition.Subscribe(_ => _condition.OnNext(Unit.Default));

            BuildPermanentDisposable(conditionSubscription, _condition);
        }
    }
}