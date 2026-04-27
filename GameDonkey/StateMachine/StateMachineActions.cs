using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public class StateMachineActions
    {
        #region Properties

        public Dictionary<string, SingleStateActions> Actions { get; private set; }

        #endregion //Properties

        #region Initialization
        public StateMachineActions()
        {
            Actions = new Dictionary<string, SingleStateActions>();
        }

        public void LoadStateActions(IEnumerable<string> stateNames, StateContainerModel stateContainerModel, BaseObject owner, IStateContainer stateContainer)
        {
            for (int i = 0; i < stateContainerModel.StatesActions.Count; i++)
            {
                SingleStateActions actions = null;

                actions = new SingleStateActions();
                actions.LoadStateActions(stateContainerModel.StatesActions[i], owner, stateContainer);
                Actions[stateContainerModel.StatesActions[i].StateName] = actions;
            }

            foreach (var state in stateNames)
            {
                if (!Actions.ContainsKey(state))
                {
                    Actions[state] = new SingleStateActions()
                    {
                        StateName = state
                    };
                }
            }
        }

        public void LoadContent(IGameDonkey engine, ContentManager content)
        {
            foreach (var action in Actions)
            {
                action.Value.LoadContent(engine, content);
            }
        }
        public void AddStateMachineActions(StateMachineActions stateMachineActions)
        {
            foreach (var singleStateAction in stateMachineActions.Actions)
            {
                if (!Actions.ContainsKey(singleStateAction.Key))
                {
                    Actions.Add(singleStateAction.Key, singleStateAction.Value);
                }
            }
        }

        public void RemoveStateMachineActions(StateMachineActions stateMachineActions)
        {
            foreach (var singleStateAction in stateMachineActions.Actions)
            {
                if (Actions.ContainsKey(singleStateAction.Key))
                {
                    Actions.Remove(singleStateAction.Key);
                }
            }
        }

        public void RemoveStateMachineActions(StateContainerModel stateMachineActions)
        {
            foreach (var singleStateAction in stateMachineActions.StatesActions)
            {
                if (Actions.ContainsKey(singleStateAction.StateName))
                {
                    Actions.Remove(singleStateAction.StateName);
                }
            }
        }

        #endregion //Initialization

        #region Methods
        public void StateChange(string nextState)
        {
            //set the new state actions to 'not run'
            if (!string.IsNullOrEmpty(nextState))
            {
                Actions[nextState].StateChange();
            }
        }
        public void ExecuteActions(GameClock clock, string currentState)
        {
            //execute the correct action container
            Actions[currentState].ExecuteAction(clock.CurrentTime);
        }
        public bool IsStateAttack(string state)
        {
            return Actions[state].IsAttack;
        }
        public bool IsAttackActive(GameClock clock, string currentState)
        {
            //check if the current state is an attack state, and if an attack is active
            return Actions[currentState].IsAttackActive(clock);
        }
        public void ReplaceOwner(BaseObject bot)
        {
            //replace in all the state actions
            foreach (var action in Actions)
            {
                action.Value.ReplaceOwner(bot);
            }
        }

        public SingleStateActions GetStateActions(string state)
        {
            return Actions[state];
        }

        #endregion //Methods
    }
}