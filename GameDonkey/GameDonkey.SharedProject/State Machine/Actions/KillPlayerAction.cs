using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class KillPlayerAction : BaseAction
	{
		#region Initialization

		public KillPlayerAction(BaseObject owner) :
			base(owner, EActionType.KillPlayer)
		{
		}

		public KillPlayerAction(BaseObject owner, KillPlayerActionModel actionModel) :
			base(owner, actionModel)
		{
		}

		public KillPlayerAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as KillPlayerActionModel)
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
			Owner.KillPlayer();

			return base.Execute();
		}

		#endregion //Methods
	}
}