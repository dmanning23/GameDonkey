using FilenameBuddy;
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	/// <summary>
	/// This object stores all the information to load a level object
	/// </summary>
	public class LevelObjectModel : BaseObjectModel
	{
		#region Members

		public float Size { get; private set; }
		public Vector2 Position { get; private set; }

		#endregion //Members

		#region Methods

		public LevelObjectModel(Filename filename) : base("levelObject", filename)
		{
			Size = 1.0f;
			Position = Vector2.Zero;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "size":
					{
						//set the scale
						Size = Convert.ToSingle(value);
					}
					break;
				case "location":
					{
						//set teh position
						Position = Vector2Ext.ToVector2(value);
					}
					break;
				default:
					{
						//punt to the base class
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		#endregion //Methods
	}
}
