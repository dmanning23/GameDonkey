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

		#region Initialization

		public DeactivateActionModel()
		{
		}

		public DeactivateActionModel(DeactivateAction action) : base(action)
		{
		}

		public DeactivateActionModel(BaseAction action) : this(action as DeactivateAction)
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
