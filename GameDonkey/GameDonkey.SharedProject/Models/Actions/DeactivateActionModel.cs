using System.Xml;

namespace GameDonkeyLib
{
	public class DeactivateActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Deactivate;
			}
		}

		#endregion //Properties

		#region Methods

		public DeactivateActionModel()
		{
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as DeactivateActionModel;
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
