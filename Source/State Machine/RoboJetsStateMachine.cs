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
		Neutral = 0,
		Transforming,
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
		RobotSitStillShootForward = EState.NumStates,
		RobotFlyForwardShootForward,
		RobotFlyBackwardShootForward,
		RobotFlyUpShootForward,
		RobotFlyDownShootForward,
		RobotSitStillShootUp,
		RobotFlyForwardShootUp,
		RobotFlyBackwardShootUp,
		RobotFlyUpShootUp,
		RobotFlyDownShootUp,
		RobotSitStillShootDown,
		RobotFlyForwardShootDown,
		RobotFlyBackwardShootDown,
		RobotFlyUpShootDown,
		RobotFlyDownShootDown,
		RobotSitStillShootBackward,
		RobotFlyForwardShootBackward,
		RobotFlyBackwardShootBackward,
		RobotFlyUpShootBackward,
		RobotFlyDownShootBackward,
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

		//Left thumbstick
		Up,
		Down,
		Forward,
		Back,
		UpRelease,
		DownRelease,
		ForwardRelease,
		BackRelease,

		//These are all for the right thumbstick
		UpRight,
		DownRight,
		ForwardRight,
		BackRight,
		UpReleaseRight,
		DownReleaseRight,
		ForwardReleaseRight,
		BackReleaseRight,

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
					SetEntry(i, (int)EMessage.Done, (int)ERobotState.RobotSitStillShootForward);
				}
			}

			//Setup some hard coded entries
			if (bJetStateMachine)
			{
			}
			else
			{
				#region RobotSitStillShootForward
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.UpRight, (int)ERobotState.RobotSitStillShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.DownRight, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.ForwardRight, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.BackRight, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootForward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				#endregion
				#region RobotSitStillShootUp
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.UpRight, (int)ERobotState.RobotSitStillShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.DownRight, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.ForwardRight, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.BackRight, (int)ERobotState.RobotSitStillShootBackward);
				SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootUp, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				#endregion
				#region RobotSitStillShootDown
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.UpRight, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.DownRight, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.ForwardRight, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.BackRight, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootDown, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				#endregion
				#region RobotSitStillShootBackward
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootBackward);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.UpRight, (int)ERobotState.RobotSitStillShootUp);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.DownRight, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.ForwardRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.BackRight, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotSitStillShootBackward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotSitStillShootForward);
				#endregion

				#region RobotFlyForwardShootForward
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyForwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyForwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootForward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion //RobotSitStillShootForward
				#region RobotFlyForwardShootUp
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.UpRight, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.DownRight, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.BackRight, (int)ERobotState.RobotFlyForwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootUp, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootUp);
				#endregion
				#region RobotFlyForwardShootDown
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.UpRight, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.DownRight, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.BackRight, (int)ERobotState.RobotFlyForwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyForwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootDown, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootDown);
				#endregion
				#region RobotFlyForwardShootBackward
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyForwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyForwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyForwardShootBackward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootBackward);
				#endregion

				#region RobotFlyBackwardShootForward
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyBackwardShootUp);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyBackwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootForward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyBackwardShootUp
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.UpRight, (int)ERobotState.RobotFlyBackwardShootUp);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.DownRight, (int)ERobotState.RobotFlyBackwardShootDown);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.BackRight, (int)ERobotState.RobotFlyBackwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootUp, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyBackwardShootDown
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootDown);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.UpRight, (int)ERobotState.RobotFlyBackwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.DownRight, (int)ERobotState.RobotFlyBackwardShootDown);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.BackRight, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootDown, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyBackwardShootBackward
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.BackRelease, (int)ERobotState.RobotSitStillShootBackward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyBackwardShootUp);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyBackwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyBackwardShootBackward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				#endregion

				#region RobotFlyUpShootForward
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyUpShootDown);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootForward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyUpShootUp
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyUpShootUp);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyUpShootUp);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyUpShootUp);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.UpRight, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.DownRight, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.BackRight, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootUp, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyUpShootDown
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyUpShootDown);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyUpShootDown);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.UpRight, (int)ERobotState.RobotFlyUpShootUp);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.DownRight, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyUpShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.BackRight, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootDown, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyForwardShootForward);
				#endregion
				#region RobotFlyUpShootBackward
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.UpRelease, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.DownRelease, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyUpShootBackward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyUpShootUp);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyUpShootDown);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyBackwardShootForward);
				SetEntry((int)ERobotState.RobotFlyUpShootBackward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyUpShootForward);
				#endregion

				#region RobotFlyDownShootForward
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyDownShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootForward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				#endregion
				#region RobotFlyDownShootUp
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.UpRight, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.DownRight, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.BackRight, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootUp, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyDownShootUp);
				#endregion
				#region RobotFlyDownShootDown
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyDownShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.UpRight, (int)ERobotState.RobotFlyDownShootUp);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.DownRight, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyDownShootForward);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.BackRight, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyDownShootDown);
				//SetEntry((int)ERobotState.RobotFlyDownShootDown, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyDownShootDown);
				#endregion
				#region RobotFlyDownShootBackward
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.Up, (int)ERobotState.RobotFlyUpShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.Down, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.Forward, (int)ERobotState.RobotFlyForwardShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.Back, (int)ERobotState.RobotFlyBackwardShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.UpRelease, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.DownRelease, (int)ERobotState.RobotSitStillShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.ForwardRelease, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.BackRelease, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.UpRight, (int)ERobotState.RobotFlyDownShootUp);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.DownRight, (int)ERobotState.RobotFlyDownShootDown);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.ForwardRight, (int)ERobotState.RobotFlyDownShootForward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.BackRight, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.UpReleaseRight, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.DownReleaseRight, (int)ERobotState.RobotFlyDownShootBackward);
				//SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.ForwardReleaseRight, (int)ERobotState.RobotFlyDownShootBackward);
				SetEntry((int)ERobotState.RobotFlyDownShootBackward, (int)EMessage.BackReleaseRight, (int)ERobotState.RobotFlyDownShootForward);
				#endregion
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
					SetStateName(iEnumIndex, stateName);
					iEnumIndex++;
				}
			}
			else
			{
				foreach (var stateName in Enum.GetNames(typeof(ERobotState)))
				{
					SetStateName(iEnumIndex, stateName);
					iEnumIndex++;
				}
			}

			#endregion //state names
		}

		#endregion //Members
	}
}