using Microsoft.Xna.Framework;
using System.Xml;
using Vector2Extensions;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class SpawnPointModel : XmlObject
	{
		#region Properties

		public Vector2 Location { get; set; }

		#endregion //Properties

		#region Methods
		public SpawnPointModel()
		{
		}

		public SpawnPointModel(Vector2 location)
		{
			Location = location;
		}

		#endregion //Methods

		#region File IO

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "location":
					{
						Location = Vector2Ext.ToVector2(value);
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
		}

		#endregion //File IO
	}
}