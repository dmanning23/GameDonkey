using System.Diagnostics;
using SPFSettings;
using GameTimer;
using Microsoft.Xna.Framework.Content;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using FilenameBuddy;
using System.Xml;

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

		#region State Action File IO

		public override bool ReadXmlStateMachine(StateMachine machine, Filename file)
		{
			return machine.AppendXmlFile(file);
		}

		public override void ReadSerializedStateMachine(ContentManager rContent, StateMachine machine, Filename file, int iMessageOffset)
		{
			machine.AppendSerializedFile(rContent, file, iMessageOffset);
		}

		#endregion //Combined File IO
	}
}