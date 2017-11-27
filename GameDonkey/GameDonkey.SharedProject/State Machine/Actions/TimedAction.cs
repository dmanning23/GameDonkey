using GameTimer;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is like an interface for an action that is timed.
	/// </summary>
	public abstract class TimedAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// the time delta how long the action is active
		/// put -1 in here to activate it until the state ends
		/// </summary>
		private float _timeDelta;
		public float TimeDelta
		{
			get { return _timeDelta; }
			set
			{
				ActiveForWholeState = (value == -1.0f);
				_timeDelta = value;
			}
		}

		/// <summary>
		/// The time when this action is done.
		/// Set at runtime when the action is activated
		/// </summary>
		public float DoneTime { get; set; }

		/// <summary>
		/// Flag whether or not to activate for the whole state
		/// If TimeDelta is set to -1, this will be true;
		/// </summary>
		public bool ActiveForWholeState { get; private set; }

		#endregion //properties

		#region Methods

		public TimedAction(BaseObject owner, EActionType actionType) : base(owner, actionType)
		{
			TimeDelta = -1f;
			DoneTime = 0f;
		}

		protected TimedAction(BaseObject owner, BaseActionModel actionModel) : base(owner, actionModel)
		{
			TimeDelta = -1f;
			DoneTime = 0f;
		}

		protected TimedAction(BaseObject owner, BaseActionModel actionModel, TimedActionModel timeActionModel) : this(owner, actionModel)
		{
			TimeDelta = timeActionModel.TimeDelta;
		}

		/// <summary>
		/// Set the done time of this action.
		/// Should be called during the Execute method of the child class
		/// </summary>
		/// <param name="clock"></param>
		public void SetDoneTime(GameClock clock)
		{
			//activate the attack
			DoneTime = clock.CurrentTime + TimeDelta;
		}

		#endregion
	}
}
