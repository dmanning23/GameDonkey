using System;
using StateMachineBuddy;

namespace GameDonkey
{
	/// <summary>
	/// This is a single state container that can be used by an object
	/// It signs up for all the events of the state machine, which is the only difference
	/// </summary>
	class ObjectStateContainer : CSingleStateContainer
	{
		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="myStateMachine">the state machine this dude will use</param>
		public ObjectStateContainer(StateMachine myStateMachine)
			: base(myStateMachine)
		{
			myStateMachine.StateChangedEvent += this.StateChange;
		}
	}
}
