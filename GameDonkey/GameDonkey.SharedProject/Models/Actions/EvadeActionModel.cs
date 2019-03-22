using System.Xml;

namespace GameDonkeyLib
{
	public class EvadeActionModel : BaseActionModel, ITimedActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Evade;
			}
		}

		public TimedActionModel TimeDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public EvadeActionModel()
		{
			TimeDelta = new TimedActionModel();
		}

		public EvadeActionModel(EvadeAction action) : base(action)
		{
			TimeDelta = new TimedActionModel(action);
		}

		public EvadeActionModel(BaseAction action) : this(action as EvadeAction)
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
				case "timedelta":
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
			TimeDelta.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
