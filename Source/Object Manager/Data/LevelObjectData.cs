using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkey
{
	/// <summary>
	/// This class is used to load level object data from xml for the tools
	/// </summary>
	public class LevelObjectData : BaseObjectData
	{
		#region Members

		public float Size { get; private set; }
		public Vector2 Position { get; private set; }

		#endregion //Members

		#region Methods

		public LevelObjectData()
		{
			Size = 1.0f;
			Position = Vector2.Zero;
		}

		protected override bool ParseXmlNode(XmlNode childNode)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerXml;

			switch (strName)
			{
				case "size":
				{
					//set the scale
					Size = Convert.ToSingle(strValue);
					return true;
				}

				case "location":
				{
					//set teh position
					Position = Vector2Ext.ToVector2(strValue);
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