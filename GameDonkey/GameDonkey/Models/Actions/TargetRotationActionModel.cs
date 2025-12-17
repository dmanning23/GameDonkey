using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class TargetRotationActionModel : BaseActionModel, IDirectionalActionModel, ITimedActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.TargetRotation;
			}
		}

		public DirectionActionModel Direction { get; set; }
		public TimedActionModel TimeDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public TargetRotationActionModel()
		{
			Direction = new DirectionActionModel();
			TimeDelta = new TimedActionModel();
		}

		public TargetRotationActionModel(TargetRotationAction action) : base(action)
		{
			Direction = new DirectionActionModel(action.TargetRotation);
			TimeDelta = new TimedActionModel(action);
		}

		public TargetRotationActionModel(BaseAction action) : this(action as TargetRotationAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "targetRotation":
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
					}
					break;
				case "timeDelta":
				case "TimeDelta":
					{
						TimeDelta.ParseXmlNode(node);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			TimeDelta.WriteXmlNodes(xmlWriter);
			Direction.WriteXmlNodes(xmlWriter);
		}

		#endregion //Methods
	}
}
