using GameTimer;

namespace GameDonkeyLib
{
	public abstract class TimedAction : BaseAction
	{
		#region Properties
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
		public float DoneTime { get; set; }
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
		public void SetDoneTime(GameClock clock)
		{
			//activate the attack
			DoneTime = clock.CurrentTime + TimeDelta;
		}

		#endregion
	}
}
