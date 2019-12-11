using FilenameBuddy;
using System.Xml;

namespace GameDonkeyLib
{
	public class ShieldActionModel : BaseActionModel, ITimedActionModel, IHasStateActionsListModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Shield;
			}
		}

		public TimedActionModel TimeDelta { get; set; }

		public StateActionsListModel ActionModels { get; set; }

		#endregion //Properties

		#region Initialization

		public ShieldActionModel()
		{
			ActionModels = new StateActionsListModel();
			TimeDelta = new TimedActionModel();
		}

		public ShieldActionModel(ShieldAction action) : base(action)
		{
			TimeDelta = new TimedActionModel(action);
			ActionModels = new StateActionsListModel(action.Actions);
		}

		public ShieldActionModel(BaseAction action) : this(action as ShieldAction)
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
				case "hitsound":
					{
						if (!string.IsNullOrEmpty(value))
						{
							ActionModels.ActionModels.Add(new PlaySoundActionModel()
							{
								Filename = new Filename(value)
							});
						}
					}
					break;
				case "actions":
				case "successactions":
					{
						ActionModels.ParseXmlNode(node);
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

			ActionModels.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
