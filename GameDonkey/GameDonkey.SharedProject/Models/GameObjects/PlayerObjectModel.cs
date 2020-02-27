using FilenameBuddy;
using System.Xml;

namespace GameDonkeyLib
{
	public class PlayerObjectModel : BaseObjectModel
	{
		#region Properties

		public Filename Portrait { get; set; }
		public Filename DeathSound { get; set; }
		public Filename BlockSound { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		public PlayerObjectModel(Filename filename) : base("playerObject", filename)
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
				case "portrait":
					{
						if (!string.IsNullOrEmpty(value))
						{
							Portrait = new Filename(value);
						}
					}
					break;
				case "deathSound":
					{
						if (!string.IsNullOrEmpty(value))
						{
							DeathSound = new Filename(value);
						}
					}
					break;
				case "blockSound":
					{
						if (!string.IsNullOrEmpty(value))
						{
							BlockSound = new Filename(value);
						}
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		#endregion //File IO
	}
}
