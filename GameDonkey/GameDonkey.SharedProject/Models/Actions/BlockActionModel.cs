using System.Xml;

namespace GameDonkeyLib
{
	public class BlockActionModel : ShieldActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Block;
			}
		}

		public string BoneName { get; set; }

		#endregion //Properties

		#region Initialization

		public BlockActionModel()
		{
		}

		public BlockActionModel(BlockAction action) : base(action)
		{
			BoneName = action.BoneName;
		}

		public BlockActionModel(BaseAction action) : this(action as BlockAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name.ToLower())
			{
				case "bonename":
					{
						BoneName = value;
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

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteAttributeString("BoneName", BoneName);
			base.WriteActionXml(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
