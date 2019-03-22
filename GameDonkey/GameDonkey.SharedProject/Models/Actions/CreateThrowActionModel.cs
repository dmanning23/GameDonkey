using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class CreateThrowActionModel : CreateAttackActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateThrow;
			}
		}

		public string ThrowMessage { get; set; }
		public float ReleaseTimeDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public CreateThrowActionModel()
		{
		}

		public CreateThrowActionModel(CreateThrowAction action) : base(action)
		{
			ThrowMessage = action.ThrowMessage;
			ReleaseTimeDelta = action.ReleaseTimeDelta;
		}

		public CreateThrowActionModel(BaseAction action) : this(action as CreateThrowAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "ThrowMessage":
					{
						ThrowMessage = value;
					}
					break;
				case "ReleaseTimeDelta":
					{
						ReleaseTimeDelta = Convert.ToSingle(value);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

#if !WINDOWS_UWP

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("ThrowMessage", ThrowMessage);
			xmlWriter.WriteAttributeString("ReleaseTimeDelta", ReleaseTimeDelta.ToString());
			base.WriteActionXml(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
