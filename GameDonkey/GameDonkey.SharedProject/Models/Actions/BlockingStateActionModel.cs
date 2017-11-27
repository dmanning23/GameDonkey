using System.Xml;

namespace GameDonkeyLib
{
	public class BlockingStateActionModel : CreateBlockActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.BlockState;
			}
		}

		public string BoneName { get; set; }

		#endregion //Properties

		#region Initialization

		public BlockingStateActionModel()
		{
		}

		public BlockingStateActionModel(BlockingStateAction action) : base(action)
		{
			BoneName = action.BoneName;
		}

		public BlockingStateActionModel(BaseAction action) : this(action as BlockingStateAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as BlockingStateActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (BoneName != stateAction.BoneName)
			{
				return false;
			}

			return base.Compare(inst);
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "BoneName":
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
