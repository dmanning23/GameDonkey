using FilenameBuddy;
using System.Xml;

namespace GameDonkey
{
	/// <summary>
	/// This class is used to load player object data from xml for the tools
	/// </summary>
	public class PlayerObjectData : BaseObjectData
	{
		#region Members

		//player object data

		public Filename PortraitFile { get; private set; }
		public Filename DeathSoundFile { get; private set; }
		public Filename BlockSoundFile { get; private set; }

		#endregion //Members

		#region Methods

		public PlayerObjectData()
		{
		}

		protected override bool ParseXmlNode(XmlNode childNode)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerXml;

			switch (strName)
			{
				case "portrait":
				{
					//get the portrait file
					if (!string.IsNullOrEmpty(strValue))
					{
						PortraitFile = new Filename(strValue);
					}
					return true;
				}

				case "deathSound":
				{
					if (!string.IsNullOrEmpty(strValue))
					{
						DeathSoundFile = new Filename(strValue);
					}
					return true;
				}

				case "blockSound":
				{
					if (!string.IsNullOrEmpty(strValue))
					{
						BlockSoundFile = new Filename(strValue);
					}
					return true;
				}

				default:
				{
					//punt to the base class
					return base.ParseXmlNode(childNode);
				}
			}
		}

		#endregion //Methods
	}
}