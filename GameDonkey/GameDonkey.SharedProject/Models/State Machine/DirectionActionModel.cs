using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using XmlBuddy;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	public class DirectionActionModel : XmlObject
	{
		#region Properties

		public Vector2 Velocity { get; private set; }
		public EDirectionType DirectionType { get; private set; }

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

		public bool Compare(DirectionActionModel inst)
		{
			return Velocity.AlmostEqual(inst.Velocity) &&
				DirectionType == inst.DirectionType;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "Velocity":
					{
						Velocity = value.ToVector2();
					}
					break;
				case "DirectionType":
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

#if !WINDOWS_UWP

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Direction");
			xmlWriter.WriteAttributeString("Velocity", Velocity.StringFromVector());
			xmlWriter.WriteAttributeString("DirectionType", DirectionType.ToString());
			xmlWriter.WriteEndElement();
		}

#endif

		#endregion //Methods
	}
}
