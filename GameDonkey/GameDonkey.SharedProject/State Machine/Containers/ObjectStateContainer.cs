using StateMachineBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a single state container that can be used by an object
	/// It signs up for all the events of the state machine, which is the only difference
	/// </summary>
	class ObjectStateContainer : SingleStateContainer
	{
		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="myStateMachine">the state machine this dude will use</param>
		public ObjectStateContainer(StateMachine myStateMachine)
			: base(myStateMachine, "ObjectStateContainer")
		{
			myStateMachine.StateChangedEvent += this.StateChange;
		}
	}
}
