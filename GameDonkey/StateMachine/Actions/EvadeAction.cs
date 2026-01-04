using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class EvadeAction : TimedAction
	{
		#region Initialization

		public EvadeAction(BaseObject owner) :
			base(owner, EActionType.Evade)
		{
		}

		public EvadeAction(BaseObject owner, EvadeActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
		}

		public EvadeAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as EvadeActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//activate the attack
			Owner.EvasionTimer.Start(TimeDelta);

			return base.Execute();
		}

		#endregion //Methods
	}
}