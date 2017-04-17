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
		/// <param name="rAction">action to add</param>
		public virtual void AddAction(T rAction, GameClock rClock)
		{
			Debug.Assert(null != rAction);
			Debug.Assert(null != rClock);

			//set the done time
			rAction.SetDoneTime(rClock);

			//store the action
			CurrentActions.Add(rAction);
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
		/// <param name="rClock"></param>
		public virtual void Update(GameClock rClock)
		{
			//remove any finished actions from the list
			int iCurrent = 0;
			while (iCurrent < CurrentActions.Count)
			{
				//checked if this action has expired...
				if (!CurrentActions[iCurrent].ActiveForWholeState &&
					(CurrentActions[iCurrent].DoneTime <= rClock.CurrentTime))
				{
					CurrentActions.RemoveAt(iCurrent);
				}
				else
				{
					iCurrent++;
				}
			}
		}

		#endregion //Methods
	}
}
