using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class AddVelocityActionModel : BaseActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.AddVelocity;
			}
		}

		public DirectionActionModel Direction { get; private set; }

		#endregion //Properties

		#region Initialization

		public AddVelocityActionModel()
		{
			Direction = new DirectionActionModel();
		}

		public AddVelocityActionModel(AddVelocityAction action) : base(action)
		{
			Direction = new DirectionActionModel(action.Velocity);
		}

		public AddVelocityActionModel(BaseAction action) : this(action as AddVelocityAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as AddVelocityActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!Direction.Compare(stateAction.Direction))
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
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
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
			Direction.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
