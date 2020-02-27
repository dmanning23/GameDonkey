using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class DirectionActionModel : XmlObject
	{
		#region Properties

		public Vector2 Velocity { get; set; }
		public EDirectionType DirectionType { get; set; }

		#endregion //Properties

		#region Initialization

		public DirectionActionModel()
		{
		}

		public DirectionActionModel(ActionDirection action)
		{
			Velocity = action.Velocity;
			DirectionType = action.DirectionType;
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
				case "#text":
				case "velocity":
					{
						Velocity = value.ToVector2();
					}
					break;
				case "directiontype":
					{
						DirectionType = (EDirectionType)Enum.Parse(typeof(EDirectionType), value);
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
			xmlWriter.WriteStartElement("Direction");
			xmlWriter.WriteAttributeString("Velocity", Velocity.StringFromVector());
			xmlWriter.WriteAttributeString("DirectionType", DirectionType.ToString());
			xmlWriter.WriteEndElement();
		}

		#endregion //Methods
	}
}
