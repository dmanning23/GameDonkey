using GameTimer;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is like an interface for an action that is timed.
	/// </summary>
	public abstract class TimedAction : BaseAction
	{
		#region Members

		/// <summary>
		/// the time delta how long the action is active
		/// put -1 in here to activate it until the state ends
		/// </summary>
		private float m_fTimeDelta;

		/// <summary>
		/// The time when this action is done.
		/// Set at runtime when the action is activated
		/// </summary>
		private float m_fDoneTime;

		/// <summary>
		/// Flag whether or not to activate for the whole state
		/// If TimeDelta is set to -1, this will be true;
		/// </summary>
		private bool m_bActiveForWholeState;

		#endregion

		#region Properties

		public float TimeDelta
		{
			get { return m_fTimeDelta; }
			set
			{
				m_bActiveForWholeState = (value == -1.0f);
				m_fTimeDelta = value;
			}
		}

		public float DoneTime
		{
			get { return m_fDoneTime; }
			private set { m_fDoneTime = value; }
		}

		public bool ActiveForWholeState
		{
			get { return m_bActiveForWholeState; }
		}

		#endregion //properties

		#region Methods

		public TimedAction(BaseObject rOwner) : base(rOwner)
		{
			TimeDelta = -1.0f;
			DoneTime = 0.0f;
		}

		/// <summary>
		/// Set the done time of this action.
		/// Should be called during the Execute method of the child class
		/// </summary>
		/// <param name="rClock"></param>
		public void SetDoneTime(GameClock rClock)
		{
			//activate the attack
			DoneTime = rClock.CurrentTime + m_fTimeDelta;
		}

		public override bool Compare(BaseAction rInst)
		{
			Debug.Assert(false);
			return false;
		}

		#endregion
	}
}
