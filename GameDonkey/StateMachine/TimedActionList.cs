using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
	public class TimedActionList<T> where T : TimedAction
	{
		#region Properties
		public List<T> CurrentActions { get; private set; }

		#endregion //Properties

		#region Methods
		public TimedActionList()
		{
			CurrentActions = new List<T>();
		}
		public virtual void AddAction(T action, GameClock clock)
		{
			//set the done time
			action.SetDoneTime(clock);

			//store the action
			CurrentActions.Add(action);
		}
		public virtual void Reset()
		{
			//remove all the actions
			CurrentActions.Clear();
		}
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
