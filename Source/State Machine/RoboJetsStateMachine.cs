using StateMachineBuddy;
using System;

namespace GameDonkey
{
	#region State Machine Enums

	/// <summary>
	/// list of all the common states between all sparrow hawk state machines
	/// </summary>
	public enum EState
	{
		Transforming = 0,
		SwitchToRobotStateMachine,
		SwitchToJetStateMachine,
		Stunned,
		TurningAround,
		NumStates
	}

	/// <summary>
	/// A list of the states that are appended to flying state machines
	/// </summary>
	public enum ERobotState
	{
		SitStillShootForward = EState.NumStates,
		FlyForwardShootForward,
		FlyBackwardShootForward,
		FlyUpShootForward,
		FlyDownShootForward,
		
		SitStillShootUp,
		FlyForwardShootUp,
		FlyBackwardShootUp,
		FlyUpShootUp,
		FlyDownShootUp,

		SitStillShootDown,
		FlyForwardShootDown,
		FlyBackwardShootDown,
		FlyUpShootDown,
		FlyDownShootDown,

		SitStillShootBackward,
		FlyForwardShootBackward,
		FlyBackwardShootBackward,
		FlyUpShootBackward,
		FlyDownShootBackward,

		SitStillShootUpForward,
		FlyForwardShootUpForward,
		FlyBackwardShootUpForward,
		FlyUpShootUpForward,
		FlyDownShootUpForward,

		SitStillShootDownForward,
		FlyForwardShootDownForward,
		FlyBackwardShootDownForward,
		FlyUpShootDownForward,
		FlyDownShootDownForward,

		SitStillShootUpBack,
		FlyForwardShootUpBack,
		FlyBackwardShootUpBack,
		FlyUpShootUpBack,
		FlyDownShootUpBack,

		SitStillShootDownBack,
		FlyForwardShootDownBack,
		FlyBackwardShootDownBack,
		FlyUpShootDownBack,
		FlyDownShootDownBack,

		NumRobotStates
	}

	/// <summary>
	/// A list of the states that are appended to groud state machines
	/// </summary>
	public enum EJetState
	{
		JetFlyNeutralNoTurn = EState.NumStates,
		JetFlyNeutralTurnUp,
		JetFlyNeutralTurnDown,
		JetAfterburnerNoTurn,
		JetAfterburnerTurnUp,
		JetAfterburnerTurnDown,
		JetBrakeNoTurn,
		JetBrakeTurnUp,
		JetBrakeTurnDown,
		JetBankUpNoTurn,
		JetBankUpTurnUp,
		JetBankUpTurnDown,
		JetBankDownNoTurn,
		JetBankDownTurnUp,
		JetBankDownTurnDown,
		NumJetStates
	}

	/// <summary>
	/// list of all the common messages between all sparrow hawk state machines
	/// </summary>
	public enum EMessage
	{
		Transform = 0,
		Done,
		Hit,
		TurnAround,

		SitStillShootForward,
		FlyForwardShootForward,
		FlyBackwardShootForward,
		FlyUpShootForward,
		FlyDownShootForward,

		SitStillShootUp,
		FlyForwardShootUp,
		FlyBackwardShootUp,
		FlyUpShootUp,
		FlyDownShootUp,

		SitStillShootDown,
		FlyForwardShootDown,
		FlyBackwardShootDown,
		FlyUpShootDown,
		FlyDownShootDown,

		SitStillShootBackward,
		FlyForwardShootBackward,
		FlyBackwardShootBackward,
		FlyUpShootBackward,
		FlyDownShootBackward,

		SitStillShootUpForward,
		FlyForwardShootUpForward,
		FlyBackwardShootUpForward,
		FlyUpShootUpForward,
		FlyDownShootUpForward,

		SitStillShootDownForward,
		FlyForwardShootDownForward,
		FlyBackwardShootDownForward,
		FlyUpShootDownForward,
		FlyDownShootDownForward,

		SitStillShootUpBack,
		FlyForwardShootUpBack,
		FlyBackwardShootUpBack,
		FlyUpShootUpBack,
		FlyDownShootUpBack,

		SitStillShootDownBack,
		FlyForwardShootDownBack,
		FlyBackwardShootDownBack,
		FlyUpShootDownBack,
		FlyDownShootDownBack,

		NumMessages
	}

	#endregion //State Machine Enums

	public class RoboJetsStateMachine : StateMachine
	{
		#region Members

		public RoboJetsStateMachine(bool bJetStateMachine)
			: base()
		{
			//setup the size of the state machine
			SetStateMachineSize(bJetStateMachine);

			//Set up all the state and message names
			SetStateAndMessageNames(bJetStateMachine);

			//setup done messages
			if (bJetStateMachine)
			{
				for (int i = 0; i < (int)EJetState.NumJetStates; i++)
				{
					SetEntry(i, (int)EMessage.Done, (int)EJetState.JetFlyNeutralNoTurn);
				}
			}
			else
			{
				for (int i = 0; i < (int)ERobotState.NumRobotStates; i++)
				{
					SetEntry(i, (int)EMessage.Done, (int)ERobotState.SitStillShootForward);
				}
			}

			//Setup some hard coded entries
			if (bJetStateMachine)
			{
				for (int i = (int)EState.NumStates; i < (int)EJetState.NumJetStates; i++)
				{
					SetEntry(i, (int)EMessage.TurnAround, (int)EState.TurningAround);
				}
			}
			else
			{
				for (int i = (int)EState.NumStates; i < (int)ERobotState.NumRobotStates; i++)
				{
					SetEntry(i, (int)EMessage.TurnAround, (int)EState.TurningAround);

					SetEntry(i, (int)EMessage.SitStillShootForward, (int)ERobotState.SitStillShootForward);
					SetEntry(i, (int)EMessage.FlyForwardShootForward, (int)ERobotState.FlyForwardShootForward);
					SetEntry(i, (int)EMessage.FlyBackwardShootForward, (int)ERobotState.FlyBackwardShootForward);
					SetEntry(i, (int)EMessage.FlyUpShootForward, (int)ERobotState.FlyUpShootForward);
					SetEntry(i, (int)EMessage.FlyDownShootForward, (int)ERobotState.FlyDownShootForward);
					SetEntry(i, (int)EMessage.SitStillShootUp, (int)ERobotState.SitStillShootUp);
					SetEntry(i, (int)EMessage.FlyForwardShootUp, (int)ERobotState.FlyForwardShootUp);
					SetEntry(i, (int)EMessage.FlyBackwardShootUp, (int)ERobotState.FlyBackwardShootUp);
					SetEntry(i, (int)EMessage.FlyUpShootUp, (int)ERobotState.FlyUpShootUp);
					SetEntry(i, (int)EMessage.FlyDownShootUp, (int)ERobotState.FlyDownShootUp);
					SetEntry(i, (int)EMessage.SitStillShootDown, (int)ERobotState.SitStillShootDown);
					SetEntry(i, (int)EMessage.FlyForwardShootDown, (int)ERobotState.FlyForwardShootDown);
					SetEntry(i, (int)EMessage.FlyBackwardShootDown, (int)ERobotState.FlyBackwardShootDown);
					SetEntry(i, (int)EMessage.FlyUpShootDown, (int)ERobotState.FlyUpShootDown);
					SetEntry(i, (int)EMessage.FlyDownShootDown, (int)ERobotState.FlyDownShootDown);
					SetEntry(i, (int)EMessage.SitStillShootBackward, (int)ERobotState.SitStillShootBackward);
					SetEntry(i, (int)EMessage.FlyForwardShootBackward, (int)ERobotState.FlyForwardShootBackward);
					SetEntry(i, (int)EMessage.FlyBackwardShootBackward, (int)ERobotState.FlyBackwardShootBackward);
					SetEntry(i, (int)EMessage.FlyUpShootBackward, (int)ERobotState.FlyUpShootBackward);
					SetEntry(i, (int)EMessage.FlyDownShootBackward, (int)ERobotState.FlyDownShootBackward);
					SetEntry(i, (int)EMessage.SitStillShootUpForward, (int)ERobotState.SitStillShootUpForward);
					SetEntry(i, (int)EMessage.FlyForwardShootUpForward, (int)ERobotState.FlyForwardShootUpForward);
					SetEntry(i, (int)EMessage.FlyBackwardShootUpForward, (int)ERobotState.FlyBackwardShootUpForward);
					SetEntry(i, (int)EMessage.FlyUpShootUpForward, (int)ERobotState.FlyUpShootUpForward);
					SetEntry(i, (int)EMessage.FlyDownShootUpForward, (int)ERobotState.FlyDownShootUpForward);
					SetEntry(i, (int)EMessage.SitStillShootDownForward, (int)ERobotState.SitStillShootDownForward);
					SetEntry(i, (int)EMessage.FlyForwardShootDownForward, (int)ERobotState.FlyForwardShootDownForward);
					SetEntry(i, (int)EMessage.FlyBackwardShootDownForward, (int)ERobotState.FlyBackwardShootDownForward);
					SetEntry(i, (int)EMessage.FlyUpShootDownForward, (int)ERobotState.FlyUpShootDownForward);
					SetEntry(i, (int)EMessage.FlyDownShootDownForward, (int)ERobotState.FlyDownShootDownForward);
					SetEntry(i, (int)EMessage.SitStillShootUpBack, (int)ERobotState.SitStillShootUpBack);
					SetEntry(i, (int)EMessage.FlyForwardShootUpBack, (int)ERobotState.FlyForwardShootUpBack);
					SetEntry(i, (int)EMessage.FlyBackwardShootUpBack, (int)ERobotState.FlyBackwardShootUpBack);
					SetEntry(i, (int)EMessage.FlyUpShootUpBack, (int)ERobotState.FlyUpShootUpBack);
					SetEntry(i, (int)EMessage.FlyDownShootUpBack, (int)ERobotState.FlyDownShootUpBack);
					SetEntry(i, (int)EMessage.SitStillShootDownBack, (int)ERobotState.SitStillShootDownBack);
					SetEntry(i, (int)EMessage.FlyForwardShootDownBack, (int)ERobotState.FlyForwardShootDownBack);
					SetEntry(i, (int)EMessage.FlyBackwardShootDownBack, (int)ERobotState.FlyBackwardShootDownBack);
					SetEntry(i, (int)EMessage.FlyUpShootDownBack, (int)ERobotState.FlyUpShootDownBack);
					SetEntry(i, (int)EMessage.FlyDownShootDownBack, (int)ERobotState.FlyDownShootDownBack);
				}
			}
		}

		/// <summary>
		/// Set the size of the state machine
		/// </summary>
		/// <param name="bJetStateMachine"></param>
		protected void SetStateMachineSize(bool bJetStateMachine)
		{
			if (bJetStateMachine)
			{
				Set((int)EJetState.NumJetStates, (int)EMessage.NumMessages, (int)EState.Transforming, 0);
			}
			else
			{
				Set((int)ERobotState.NumRobotStates, (int)EMessage.NumMessages, (int)EState.Transforming, 0);
			}
		}

		protected void SetStateAndMessageNames(bool bJetStateMachine)
		{
			#region state names

			//set all the state names
			int iEnumIndex = 0;
			foreach (var stateName in Enum.GetNames(typeof(EState)))
			{
				SetStateName(iEnumIndex, stateName);
				iEnumIndex++;
			}

			//set that last state name again
			iEnumIndex--;

			if (bJetStateMachine)
			{
				foreach (var stateName in Enum.GetNames(typeof(EJetState)))
				{
					if (iEnumIndex != (int)EJetState.NumJetStates)
					{
						SetStateName(iEnumIndex, stateName);
						iEnumIndex++;
					}
				}
			}
			else
			{
				foreach (var stateName in Enum.GetNames(typeof(ERobotState)))
				{
					if (iEnumIndex != (int)ERobotState.NumRobotStates)
					{
						SetStateName(iEnumIndex, stateName);
						iEnumIndex++;
					}
				}
			}

			//Set all the message names
			iEnumIndex = 0;
			foreach (var messageName in Enum.GetNames(typeof(EMessage)))
			{
				if (iEnumIndex != (int)EMessage.NumMessages)
				{
					SetMessageName(iEnumIndex, messageName);
					iEnumIndex++;
				}
			}


			#endregion //state names
		}

		#endregion //Members
	}
}