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
		public override bool Execute()
		{
			Owner.KillPlayer();

			return base.Execute();
		}

		#endregion //Methods
	}
}