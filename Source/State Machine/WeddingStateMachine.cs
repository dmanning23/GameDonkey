using System.Diagnostics;
using StateMachineBuddy;

namespace GameDonkey
{
	#region State Machine Enums

	/// <summary>
	/// list of all the common states between all sparrow hawk state machines
	/// </summary>
	enum EState
	{
		Quick = 0,
		QuickUp,
		QuickDown,
		QuickForward,
		QuickBack,
		Strong,
		StrongUp,
		StrongDown,
		StrongForward,
		StrongBack,
		Special,
		SpecialUp,
		SpecialDown,
		SpecialForward,
		SpecialBack,
		SwitchToGroundStateMachine,
		SwitchToUpStateMachine,
		Stunned,
		Evade,
		TurningAround,
		NumStates
	}

	/// <summary>
	/// A list of the states that are appended to flying state machines
	/// </summary>
	enum EFlyingState
	{
		Falling = EState.NumStates,
		Float,
		Dive,
		Glide,
		Brake,
		FlyUp,
		FlyDown,
		FlyForward,
		FlyBack,
		NumFlyingStates
	}

	/// <summary>
	/// A list of the states that are appended to groud state machines
	/// </summary>
	enum EGroundState
	{
		Standing = EState.NumStates,
		Walking,
		Blocking,
		HighBlock,
		LowBlock,
		Braking,
		NumGroundStates
	}

	/// <summary>
	/// list of all the common messages between all sparrow hawk state machines
	/// </summary>
	enum EMessage
	{
		Quick = 0,
		QuickUp,
		QuickDown,
		QuickForward,
		QuickBack,
		Strong,
		StrongUp,
		StrongDown,
		StrongForward,
		StrongBack,
		Special,
		SpecialUp,
		SpecialDown,
		SpecialForward,
		SpecialBack,
		Done,
		Hit,
		HitGround,
		Fell, //message sent continuously when the character hasn't been touching the ground for a set amount of time
		FlyingUp, //message sent when the character is flying mostly -y or doesnt have much speed
		FlyingDown, //message sent when the character is flying mostly +y
		FlyingForward, //message sent when the character is flying mostly +x
		FlyingBack, //message sent when the character is flying mostly -x
		NeutralSpeed, //message send when the character's x & y are insignificant
		Up,
		Down,
		Forward,
		Back,
		UpRelease,
		DownRelease,
		ForwardRelease,
		BackRelease,
		Defend,
		DefendUp,
		DefendDown,
		DefendForward,
		DefendBack,
		DefendRelease,
		NumMessages
	}

	#endregion //State Machine Enums

	class CWeddingStateMachine : StateMachine
	{
		#region Members

		public CWeddingStateMachine(bool bFlyingStateMachine)
			: base()
		{
			#region State machine sizes

			//setup the size of the state machine
			if (bFlyingStateMachine)
			{
				Set((int)EFlyingState.NumFlyingStates, (int)EMessage.NumMessages, (int)EFlyingState.Falling, 0);
			}
			else
			{
				Set((int)EGroundState.NumGroundStates, (int)EMessage.NumMessages, (int)EGroundState.Standing, 0);
			}

			#endregion //State machine sizes

			#region state names

			//set all the state names
			SetStateName((int)EState.Quick, "Quick");
			SetStateName((int)EState.QuickUp, "QuickUp");
			SetStateName((int)EState.QuickDown, "QuickDown");
			SetStateName((int)EState.QuickForward, "QuickForward");
			SetStateName((int)EState.QuickBack, "QuickBack");
			SetStateName((int)EState.Strong, "Strong");
			SetStateName((int)EState.StrongUp, "StrongUp");
			SetStateName((int)EState.StrongDown, "StrongDown");
			SetStateName((int)EState.StrongForward, "StrongForward");
			SetStateName((int)EState.StrongBack, "StrongBack");
			SetStateName((int)EState.Special, "Special");
			SetStateName((int)EState.SpecialUp, "SpecialUp");
			SetStateName((int)EState.SpecialDown, "SpecialDown");
			SetStateName((int)EState.SpecialForward, "SpecialForward");
			SetStateName((int)EState.SpecialBack, "SpecialBack");
			SetStateName((int)EState.SwitchToGroundStateMachine, "SwitchToGroundStateMachine");
			SetStateName((int)EState.SwitchToUpStateMachine, "SwitchToUpStateMachine");
			SetStateName((int)EState.Stunned, "Stunned");
			SetStateName((int)EState.Evade, "Evade");
			SetStateName((int)EState.TurningAround, "TurningAround");

			if (bFlyingStateMachine)
			{
				SetStateName((int)EFlyingState.Falling, "Falling");
				SetStateName((int)EFlyingState.Float, "Float");
				SetStateName((int)EFlyingState.Dive, "Dive");
				SetStateName((int)EFlyingState.Glide, "Glide");
				SetStateName((int)EFlyingState.Brake, "Brake");
				SetStateName((int)EFlyingState.FlyUp, "FlyUp");
				SetStateName((int)EFlyingState.FlyDown, "FlyDown");
				SetStateName((int)EFlyingState.FlyForward, "FlyForward");
				SetStateName((int)EFlyingState.FlyBack, "FlyBack");
			}
			else
			{
				SetStateName((int)EGroundState.Standing, "Standing");
				SetStateName((int)EGroundState.Walking, "Walking");
				SetStateName((int)EGroundState.Blocking, "Blocking");
				SetStateName((int)EGroundState.HighBlock, "HighBlock");
				SetStateName((int)EGroundState.LowBlock, "LowBlock");
				SetStateName((int)EGroundState.Braking, "Braking");
			}

			#endregion //state names

			#region message names

			//set all the message names
			SetMessageName((int)EMessage.Quick, "Quick");
			SetMessageName((int)EMessage.QuickUp, "QuickUp");
			SetMessageName((int)EMessage.QuickDown, "QuickDown");
			SetMessageName((int)EMessage.QuickForward, "QuickForward");
			SetMessageName((int)EMessage.QuickBack, "QuickBack");
			SetMessageName((int)EMessage.Strong, "Strong");
			SetMessageName((int)EMessage.StrongUp, "StrongUp");
			SetMessageName((int)EMessage.StrongDown, "StrongDown");
			SetMessageName((int)EMessage.StrongForward, "StrongForward");
			SetMessageName((int)EMessage.StrongBack, "StrongBack");
			SetMessageName((int)EMessage.Special, "Special");
			SetMessageName((int)EMessage.SpecialUp, "SpecialUp");
			SetMessageName((int)EMessage.SpecialDown, "SpecialDown");
			SetMessageName((int)EMessage.SpecialForward, "SpecialForward");
			SetMessageName((int)EMessage.SpecialBack, "SpecialBack");
			SetMessageName((int)EMessage.Done, "Done");
			SetMessageName((int)EMessage.Hit, "Hit");
			SetMessageName((int)EMessage.HitGround, "HitGround");
			SetMessageName((int)EMessage.Fell, "Fell");
			SetMessageName((int)EMessage.FlyingUp, "FlyingUp");
			SetMessageName((int)EMessage.FlyingDown, "FlyingDown");
			SetMessageName((int)EMessage.FlyingForward, "FlyingForward");
			SetMessageName((int)EMessage.FlyingBack, "FlyingBack");
			SetMessageName((int)EMessage.NeutralSpeed, "NeutralSpeed");
			SetMessageName((int)EMessage.Up, "Up");
			SetMessageName((int)EMessage.Down, "Down");
			SetMessageName((int)EMessage.Forward, "Forward");
			SetMessageName((int)EMessage.Back, "Back");
			SetMessageName((int)EMessage.UpRelease, "UpRelease");
			SetMessageName((int)EMessage.DownRelease, "DownRelease");
			SetMessageName((int)EMessage.ForwardRelease, "ForwardRelease");
			SetMessageName((int)EMessage.BackRelease, "BackRelease");
			SetMessageName((int)EMessage.Defend, "Defend");
			SetMessageName((int)EMessage.DefendUp, "DefendUp");
			SetMessageName((int)EMessage.DefendDown, "DefendDown");
			SetMessageName((int)EMessage.DefendForward, "DefendForward");
			SetMessageName((int)EMessage.DefendBack, "DefendBack");
			SetMessageName((int)EMessage.DefendRelease, "DefendRelease");

			#endregion //message names

			//Setup some hard coded entries

			#region Attack Cancelling

			//Set the attack messages to cancel into each other
			for (int iState = (int)EState.Quick; iState <= (int)EState.SpecialBack; iState++)
			{
				for (int iMessage = (int)EMessage.Quick; iMessage <= (int)EMessage.SpecialBack; iMessage++)
				{
					//can cheat and use the message as the state, because the enumerations match up
					SetEntry(iState, iMessage, iMessage);
				}
			}

			#endregion Attack Cancelling

			//set up the flying states
			if (bFlyingStateMachine)
			{
				#region Flying States

				//set all the done messages to go to "falling"
				for (int iState = 0; iState <= (int)EFlyingState.Brake; iState++)
				{
					SetEntry(iState, (int)EMessage.Done, (int)EFlyingState.Falling);
				}

				//setup attack messages
				for (int iState = (int)EFlyingState.Falling; iState < (int)EFlyingState.NumFlyingStates; iState++)
				{
					for (int iMessage = (int)EMessage.Quick; iMessage <= (int)EMessage.SpecialBack; iMessage++)
					{
						SetEntry(iState, iMessage, iMessage);
					}
				}

				//setup the hit state
				for (int iState = 0; iState < (int)EFlyingState.NumFlyingStates; iState++)
				{
					SetEntry(iState, (int)EMessage.Hit, (int)EState.Stunned);
				}

				//setup the dash messages

				//setup the glide messages
				for (EFlyingState iState = EFlyingState.Falling; iState <= EFlyingState.Brake; iState++)
				{
					SetEntry((int)iState, (int)EMessage.Up, (int)EFlyingState.Float);
					SetEntry((int)iState, (int)EMessage.Down, (int)EFlyingState.Dive);
					SetEntry((int)iState, (int)EMessage.Forward, (int)EFlyingState.Glide);
					//SetEntry((int)EFlyingState.Falling, (int)EMessage.Back, (int)EFlyingState.Brake);
				}

				//setup the "done gliding" messages
				SetEntry((int)EFlyingState.Float, (int)EMessage.UpRelease, (int)EFlyingState.Falling);
				SetEntry((int)EFlyingState.Dive, (int)EMessage.DownRelease, (int)EFlyingState.Falling);
				SetEntry((int)EFlyingState.Glide, (int)EMessage.ForwardRelease, (int)EFlyingState.Falling);
				SetEntry((int)EFlyingState.Brake, (int)EMessage.BackRelease, (int)EFlyingState.Falling);

				for (int iState = (int)EFlyingState.Falling; iState <= (int)EFlyingState.Brake; iState++)
				{
					//setup the dash messages
					int iMessage = (int)EMessage.DefendUp;
					int iTargetState = (int)EFlyingState.FlyUp;
					while (iMessage <= (int)EMessage.DefendBack)
					{
						SetEntry(iState, iMessage, iTargetState);
						iMessage++;
						iTargetState++;
					}

					//setup the evade message
					SetEntry(iState, (int)EMessage.Defend, (int)EState.Evade);
				}

				//setup the "done air dashing" messages
				SetEntry((int)EFlyingState.FlyUp, (int)EMessage.Done, (int)EFlyingState.Float);
				SetEntry((int)EFlyingState.FlyDown, (int)EMessage.Done, (int)EFlyingState.Dive);
				SetEntry((int)EFlyingState.FlyForward, (int)EMessage.Done, (int)EFlyingState.Glide);
				SetEntry((int)EFlyingState.FlyBack, (int)EMessage.Done, (int)EFlyingState.Brake);

				#endregion Flying States
			}
			else
			{
				#region Ground States

				//set all the done messages to go to "standing"
				for (int iState = 0; iState < (int)EGroundState.NumGroundStates; iState++)
				{
					SetEntry(iState, (int)EMessage.Done, (int)EGroundState.Standing);
				}

				for (int iState = (int)EGroundState.Standing; iState < (int)EGroundState.NumGroundStates; iState++)
				{
					//set the fell message
					SetEntry(iState, (int)EMessage.Fell, (int)EState.SwitchToUpStateMachine);

					//setup attack messages
					for (int iMessage = (int)EMessage.Quick; iMessage <= (int)EMessage.SpecialBack; iMessage++)
					{
						SetEntry(iState, iMessage, iMessage);
					}
				}

				//setup the hit state
				for (int iState = 0; iState < (int)EGroundState.NumGroundStates; iState++)
				{
					SetEntry(iState, (int)EMessage.Hit, (int)EState.Stunned);
				}

				#endregion //Ground States
			}
		}

		#endregion //Members
	}
}