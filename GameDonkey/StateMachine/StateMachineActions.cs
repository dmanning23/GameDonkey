using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// A list of all the state actions for a single state machine.
	/// </summary>
	public class StateMachineActions
	{
		#region Properties

		public Dictionary<string, SingleStateActions> Actions { get; private set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// standard constructor
		/// </summary>
		public StateMachineActions()
		{
			Actions = new Dictionary<string, SingleStateActions>();
		}

		public void LoadStateActions(IEnumerable<string> stateNames, SingleStateContainerModel stateContainerModel, BaseObject owner, IStateContainer stateContainer)
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

		/// <summary>
		/// Add one list of state machine actions into this one
		/// </summary>
		/// <param name="stateMachineActions"></param>
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

		public void RemoveStateMachineActions(SingleStateContainerModel stateMachineActions)
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

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="nextState">the new state of the object</param>
		public void StateChange(string nextState)
		{
			//set the new state actions to 'not run'
			Actions[nextState].StateChange();
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public void ExecuteActions(GameClock clock, string currentState)
		{
			//execute the correct action container
			Actions[currentState].ExecuteAction(clock.PreviousTime, clock.CurrentTime);
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="state">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(string state)
		{
			return Actions[state].IsAttack;
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive(GameClock clock, string currentState)
		{
			//check if the current state is an attack state, and if an attack is active
			return Actions[currentState].IsAttackActive(clock);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
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