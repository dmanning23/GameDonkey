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
	/// This class is used to load projectile data from xml for the tools
	/// </summary>
	public class ProjectileObjectData : BaseObjectData
	{
		#region Members

		//projectile data

		public bool Weaponhits { get; private set; }

		#endregion //Members

		#region Methods

		public ProjectileObjectData()
		{
			Weaponhits = false;
		}

		protected override bool ParseXmlNode(XmlNode childNode)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerXml;

			switch (strName)
			{
				case "weaponhits":
				{
					Weaponhits = Convert.ToBoolean(strValue);
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