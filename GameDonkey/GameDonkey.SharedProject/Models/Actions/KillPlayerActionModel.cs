using System.Xml;

namespace GameDonkeyLib
{
	public class KillPlayerActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.KillPlayer;
			}
		}

		#endregion //Properties

		#region Initialization

		public KillPlayerActionModel()
		{
		}

		public KillPlayerActionModel(KillPlayerAction action) : base(action)
		{
		}

		public KillPlayerActionModel(BaseAction action) : this(action as KillPlayerAction)
		{
		}

		#endregion //Initialization

		#region Methods

#if !WINDOWS_UWP

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
		}

#endif

		#endregion //Methods
	}
}
