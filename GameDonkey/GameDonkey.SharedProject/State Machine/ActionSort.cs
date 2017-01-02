using System.Collections.Generic;

namespace GameDonkey
{
	/// <summary>
	/// class for sorting actions in a 
	/// </summary>
	class ActionSort : IComparer<BaseAction>
	{
		public int Compare(BaseAction action1, BaseAction action2)
		{
			if (action1.Time != action2.Time)
			{
				return action1.Time.CompareTo(action2.Time);
			}
			else
			{
				return action1.ActionType.CompareTo(action2.ActionType);
			}
		}
	}
}
