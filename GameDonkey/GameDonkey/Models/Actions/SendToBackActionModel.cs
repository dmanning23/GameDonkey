
using System.Xml;

namespace GameDonkeyLib
{
	public class SendToBackActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.SendToBack;
			}
		}

		#endregion //Properties

		#region Initialization

		public SendToBackActionModel()
		{
		}

		public SendToBackActionModel(SendToBackAction action) : base(action)
		{
		}

		public SendToBackActionModel(BaseAction action) : this(action as SendToBackAction)
		{
		}

		#endregion //Initialization

		#region Methods

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
		}

		#endregion //Methods
	}
}
