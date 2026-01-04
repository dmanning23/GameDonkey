using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class SingleStateActionsModel : StateActionsListModel
	{
		#region Properties

		public string StateName { get; private set; }

		#endregion //Properties

		#region Initialization

		public SingleStateActionsModel()
		{
		}

		public SingleStateActionsModel(SingleStateActions stateActions) : base(stateActions.Actions)
		{
			StateName = stateActions.StateName;
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
				case "Asset":
					{
						//skip these old ass nodes
						XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
					}
					break;
				case "Type":
					{
						//Really skip these old ass nodes
					}
					break;
				case "name":
				case "StateName":
					{
						StateName = value;
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("State");

			xmlWriter.WriteAttributeString("StateName", StateName);

			base.WriteXmlNodes(xmlWriter);

			xmlWriter.WriteEndElement();
		}

		#endregion //Methods
	}
}
