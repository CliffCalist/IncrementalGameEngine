using System;
using System.Collections.Generic;
using R3;

namespace WhiteArrow.Incremental
{
    public abstract class StateMachine<TContext, TStateID> : DisposableBase
        where TStateID : Enum
    {
        private readonly TStateID _initialStateType;
        private readonly Dictionary<TStateID, StateBase<TContext>> _stateMap;
        private readonly List<StateTransition<TStateID>> _transitions;


        public TStateID CurrentStateID { get; private set; }
        private StateBase<TContext> _currentState;



        public StateMachine(
           Dictionary<TStateID, StateBase<TContext>> stateMap,
           List<StateTransition<TStateID>> transitions,
           TStateID initialStateType)
        {
            _stateMap = stateMap ?? throw new ArgumentNullException(nameof(stateMap));
            _transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));


            if (!stateMap.ContainsKey(initialStateType))
                throw new ArgumentException($"Initial state '{initialStateType}' is not defined in the state map.");

            _initialStateType = initialStateType;
        }

        protected void SetInitialState()
        {
            ThrowIfDisposed();

            CurrentStateID = _initialStateType;
            _currentState = _stateMap[_initialStateType];
            _currentState.OnEnterState(GetContext());

            var subscriptions = new List<IDisposable>();
            foreach (var transition in _transitions)
            {
                var subscription = transition.Condition
                    .Where(_ => EqualityComparer<TStateID>.Default.Equals(CurrentStateID, transition.From))
                    .Subscribe(_ => ChangeState(transition.To));

                subscriptions.Add(subscription);
            }

            BuildPermanentDisposable(subscriptions.ToArray());
        }




        private void ChangeState(TStateID newStateType)
        {
            ThrowIfDisposed();

            if (!EqualityComparer<TStateID>.Default.Equals(CurrentStateID, newStateType))
            {
                _currentState.OnExitState(GetContext());
                _currentState = _stateMap[newStateType];
                CurrentStateID = newStateType;
                _currentState.OnEnterState(GetContext());
            }
        }

        protected abstract TContext GetContext();


        /// <summary>
        /// Get a state by interface type.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <returns>An instance of the state implementing the specified interface.</returns>
        public T GetStateAs<T>() where T : class
        {
            ThrowIfDisposed();

            foreach (var state in _stateMap.Values)
            {
                if (state is T castedState)
                    return castedState;
            }

            return null;
        }



        protected override void DisposeProtected()
        {
            foreach (var transition in _transitions)
                transition.Dispose();
            _transitions.Clear();

            foreach (var state in _stateMap.Values)
                state.Dispose();
            _stateMap.Clear();

            base.DisposeProtected();
        }
    }
}