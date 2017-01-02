using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;

namespace GameDonkey
{
	/// <summary>
	/// This a state container with one state machine
	/// Heads up, this state machine doesn't sign up for all the state change messages...
	/// It's meant to be used with playerstatecontainer, otherwise use the singlestatecontainer
	/// </summary>
	public class SingleStateAppenderContainer : SingleStateContainer
	{
		public SingleStateAppenderContainer(StateMachine myStateMachine, string containerName) : base(myStateMachine, containerName)
		{
		}

		public override bool ReadXmlStateMachine(StateMachine machine, Filename file, ContentManager content)
		{
			return machine.AppendXmlFile(file, content);
		}
	}
}