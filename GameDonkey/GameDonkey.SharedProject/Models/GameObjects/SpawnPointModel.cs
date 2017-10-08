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

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public SpawnPointModel()
		{
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
				case "Type":
					{
						//throw these attributes out
					}
					break;
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

#if !WINDOWS_UWP
		/// <summary>
		/// Write this dude out to the xml format
		/// </summary>
		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			//			//write out the item tag
			//			xmlWriter.WriteStartElement("joint");
			//			xmlWriter.WriteAttributeString("name", Name);
			//			xmlWriter.WriteEndElement();
		}
#endif

		#endregion //File IO
	}
}