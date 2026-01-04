using System.Xml;

namespace GameDonkeyLib
{
	public class SendStateMessageActionModel : BaseActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.SendStateMessage;
			}
		}

		public string Message { get; set; }

		#endregion //Properties

		#region Initialization

		public SendStateMessageActionModel()
		{
		}

		public SendStateMessageActionModel(SendStateMessageAction action) : base(action)
		{
			Message = action.Message;
		}

		public SendStateMessageActionModel(BaseAction action) : this(action as SendStateMessageAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name.ToLower())
			{
				case "message":
					{
						Message = value;
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("Message", Message);
		}

		#endregion //Methods
	}
}
