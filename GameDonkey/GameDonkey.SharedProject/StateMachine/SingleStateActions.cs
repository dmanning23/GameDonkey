using GameTimer;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	/// <summary>
	/// A list of actions to perform while in one state
	/// </summary>
	public class SingleStateActions : StateActionsList
	{
		#region Properties

		/// <summary>
		/// If this state is an attack or a throw, the time that the startup ends
		/// </summary>
		protected float ActiveTime { get; private set; }

		/// <summary>
		/// The time that this action enters the "recovery" phase and can be cancelled into other actions
		/// </summary>
		protected float RecoveryTime { get; private set; }

		/// <summary>
		/// name of the state this thing is describing
		/// </summary>
		public string StateName { get; set; }

		/// <summary>
		/// whether or not this is an attack/throw state
		/// </summary>
		public bool IsAttack { get; private set; }

		#endregion //Properties

		#region Methods

		#region Initialization

		/// <summary>
		/// standard constructor!
		/// </summary>
		public SingleStateActions()
		{
		}

		public void LoadStateActions(SingleStateActionsModel actionModels, BaseObject owner, IStateContainer stateContainer)
		{
			StateName = actionModels.StateName;
			base.LoadStateActions(actionModels, owner, stateContainer);
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			base.LoadContent(engine, content);

			//calculate "active" and "recovery" phases
			CalculateAttackTime();
		}

		#endregion //Initialization

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		public void StateChange()
		{
			for (int i = 0; i < Actions.Count; i++ )
			{
				Actions[i].AlreadyRun = false;
			}
		}

		/// <summary>
		/// Execute the actions that occur between the time slice. 
		/// Time is measured in seconds since entering the state
		/// </summary>
		/// <param name="prevTime">time of the previous frame</param>
		/// <param name="currentTime">the current time</param>
		public void ExecuteAction(float prevTime, float currentTime)
		{
			//loop through all actions, execute the ones between the time slice
			for (int i = 0; i < Actions.Count; i++)
			{
				//first check if the time of this action is expired
				if (Actions[i].Time > currentTime)
				{
					//this action doesnt need to be run yet!
					return;
				}

				//check if this action hasn't happened yet
				else if (!Actions[i].AlreadyRun)
				{
					if (Actions[i].Execute())
					{
						//the state was changed when that dude was running
						return;
					}
				}
			}
		}

		/// <summary>
		/// Calculate the startup and recovery times for this state
		/// </summary>
		private void CalculateAttackTime()
		{
			//does this state have any attack actions?
			for (int i = 0; i < Actions.Count; i++)
			{
				if ((Actions[i].ActionType == EActionType.CreateAttack) ||
					(Actions[i].ActionType == EActionType.CreateThrow) ||
					(Actions[i].ActionType == EActionType.CreateHitCircle))
				{
					//set this state to an attack state
					IsAttack = true;

					//check if this is the end of the startup 
					var attackAction = (CreateAttackAction)Actions[i];
					if (0.0f == ActiveTime)
					{
						ActiveTime = attackAction.Time;
					}

					//check if this attack is teh recovery time
					if (RecoveryTime < (attackAction.Time + attackAction.TimeDelta))
					{
						RecoveryTime = attackAction.Time + attackAction.TimeDelta;
					}
				}
				else if (Actions[i].ActionType == EActionType.Projectile)
				{
					//set this state to an attack state
					IsAttack = true;

					//check if this is the end of the startup 
					var projectileAction = (ProjectileAction)Actions[i];
					if (0.0f == ActiveTime)
					{
						ActiveTime = projectileAction.Time;
					}

					//check if this attack is teh recovery time
					if (RecoveryTime < (projectileAction.Time))
					{
						RecoveryTime = projectileAction.Time;
					}
				}
			}
		}

		public bool IsAttackActive(GameClock stateClock)
		{
			//the attacks are still active if the recovery time hasnt started
			return (stateClock.CurrentTime < RecoveryTime);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		public void ReplaceOwner(BaseObject bot)
		{
			//replace in all the state actions
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].Owner = bot;
			}
		}

		public override string ToString()
		{
			return StateName;
		}

		#endregion //Methods
	}
}