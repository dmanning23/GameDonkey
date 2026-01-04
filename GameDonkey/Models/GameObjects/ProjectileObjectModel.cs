using FilenameBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
	public class ProjectileObjectModel : InstanceObjectModel
	{
		#region Properties

		public bool Weaponhits { get; private set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public ProjectileObjectModel(Filename filename) : base(filename, "projectileObject")
		{
		}

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

		#endregion //Methods
	}
}
