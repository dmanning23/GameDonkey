using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class SetVelocityActionModel : BaseActionModel, IDirectionalActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.SetVelocity;
			}
		}

		public DirectionActionModel Direction { get; private set; }

		#endregion //Properties

		#region Methods

		public SetVelocityActionModel()
		{
			Direction = new DirectionActionModel();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as SetVelocityActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!Direction.Compare(stateAction.Direction))
			{
				return false;
			}

			return true;
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
