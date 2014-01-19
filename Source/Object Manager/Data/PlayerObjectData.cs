using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SPFSettings;

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

		#endregion //Members

		#region Methods

		public PlayerObjectData()
		{
			PortraitFile = new Filename();
			DeathSoundFile = new Filename();
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
					PortraitFile.SetRelFilename(strValue);
					return true;
				}

				case "deathSound":
				{
					DeathSoundFile.SetRelFilename(strValue);
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