using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// This class is like an interface for managing a list of timed actions
	/// </summary>
	public class TimedActionList<T> where T : TimedAction
	{
		#region Properties

		/// <summary>
		/// A list of all the actions that are currently added to the base object
		/// </summary>
		public List<T> CurrentActions { get; private set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		public TimedActionList()
		{
			CurrentActions = new List<T>();
		}

		/// <summary>
		/// Add an action to the list, set the done time
		/// Shoudl be called from the action's Execute
		/// </summary>
		/// <param name="action">action to add</param>
		public virtual void AddAction(T action, GameClock clock)
		{
			//set the done time
			action.SetDoneTime(clock);

			//store the action
			CurrentActions.Add(action);
		}

		/// <summary>
		/// remove all the actions from the list of current actions
		/// shoudl be called on reset, state change
		/// </summary>
		public virtual void Reset()
		{
			//remove all the actions
			CurrentActions.Clear();
		}

		/// <summary>
		/// Update the garment manager.  
		/// Checks if any actions are ready to be removed
		/// shoudl be called from base object's update
		/// </summary>
		/// <param name="clock"></param>
		public virtual void Update(GameClock clock)
		{
			//remove any finished actions from the list
			int i = 0;
			while (i < CurrentActions.Count)
			{
				//checked if this action has expired...
				if (!CurrentActions[i].ActiveForWholeState &&
					(CurrentActions[i].DoneTime <= clock.CurrentTime))
				{
					CurrentActions.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		#endregion //Methods
	}
}
