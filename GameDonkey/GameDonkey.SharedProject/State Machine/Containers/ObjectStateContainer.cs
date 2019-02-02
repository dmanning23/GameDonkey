using StateMachineBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a single state container that can be used by an object
	/// It signs up for all the events of the state machine, which is the only difference
	/// </summary>
	public class ObjectStateContainer : SingleStateContainer
	{
		public ObjectStateContainer(HybridStateMachine stateMachine, string containerName = "") :
			base(stateMachine, containerName)
		{
			stateMachine.StateChangedEvent += this.StateChange;
		}
	}
}
