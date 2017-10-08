using FilenameBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class ProjectileObjectModel : BaseObjectModel
	{
		#region Properties

		public bool Weaponhits { get; private set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public ProjectileObjectModel(Filename filename) : base("projectileObject", filename)
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
				case "weaponhits":
					{
						Weaponhits = Convert.ToBoolean(value);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

//#if !WINDOWS_UWP
//		/// <summary>
//		/// Write this dude out to the xml format
//		/// </summary>
//		/// <param name="xmlWriter">the xml file to add this dude as a child of</param>
//		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
//		{
//			//write out the item tag
//			xmlWriter.WriteStartElement("joint");
//			xmlWriter.WriteAttributeString("name", Name);
//			xmlWriter.WriteEndElement();
//		}
//#endif

		#endregion //File IO
	}
}
