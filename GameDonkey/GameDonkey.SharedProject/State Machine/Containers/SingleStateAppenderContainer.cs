using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;

namespace GameDonkeyLib
{
	/// <summary>
	/// This a state container with one state machine
	/// Heads up, this state machine doesn't sign up for all the state change messages...
	/// It's meant to be used with playerstatecontainer, otherwise use the singlestatecontainer
	/// </summary>
	public class SingleStateAppenderContainer : SingleStateContainer
	{

		public SingleStateAppenderContainer(StateMachine stateMachine, string containerName) :
			base(stateMachine, containerName)
		{
		}

		public override void LoadStateMachine(StateMachine machine, Filename file, ContentManager content)
		{
			machine.AppendXml(file, content);
		}
	}
}