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

		#region Methods

		public KillPlayerActionModel()
		{
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as KillPlayerActionModel;
			if (null == stateAction)
			{
				return false;
			}

			return true;
		}

#if !WINDOWS_UWP

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
		}

#endif

		#endregion //Methods
	}
}
