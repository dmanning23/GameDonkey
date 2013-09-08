using System;
using GameTimer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GameDonkey
{
	/// <summary>
	/// This class is like an interface for managing a list of timed actions
	/// </summary>
	public class TimedActionList<T> where T : TimedAction
	{
		#region Members

		/// <summary>
		/// A list of all the actions that are currently added to the base object
		/// </summary>
		private List<T> m_listCurrentActions;

		#endregion Members

		#region Properties

		public List<T> CurrentActions
		{
			get { return m_listCurrentActions; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		public TimedActionList()
		{
			m_listCurrentActions = new List<T>();
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
			m_listCurrentActions.Add(rAction);
		}

		/// <summary>
		/// remove all the actions from the list of current actions
		/// shoudl be called on reset, state change
		/// </summary>
		public virtual void Reset()
		{
			//remove all the actions
			m_listCurrentActions.Clear();
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
			while (iCurrent < m_listCurrentActions.Count)
			{
				//checked if this action has expired...
				if (!m_listCurrentActions[iCurrent].ActiveForWholeState &&
					(m_listCurrentActions[iCurrent].DoneTime <= rClock.CurrentTime))
				{
					m_listCurrentActions.RemoveAt(iCurrent);
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
