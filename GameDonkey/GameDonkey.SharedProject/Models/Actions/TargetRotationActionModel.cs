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

		public DirectionActionModel Direction { get; private set; }
		public TimedActionModel TimeDelta { get; private set; }

		#endregion //Properties

		#region Methods

		public TargetRotationActionModel()
		{
			Direction = new DirectionActionModel();
			TimeDelta = new TimedActionModel();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as TargetRotationActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!TimeDelta.Compare(stateAction.TimeDelta))
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

#if !WINDOWS_UWP

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			Direction.WriteXmlNodes(xmlWriter);
			TimeDelta.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
