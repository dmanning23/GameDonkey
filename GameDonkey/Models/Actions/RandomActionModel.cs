using System.Xml;

namespace GameDonkeyLib
{
	public class RandomActionModel : BaseActionModel, IHasStateActionsListModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.Random;
			}
		}

		public StateActionsListModel ActionModels { get; set; }

		#endregion //Properties

		#region Initialization

		public RandomActionModel()
		{
			ActionModels = new StateActionsListModel();
		}

		public RandomActionModel(RandomAction action) : base(action)
		{
			ActionModels = new StateActionsListModel(action.Actions);
		}

		public RandomActionModel(BaseAction action) : this(action as RandomAction)
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
				case "actions":
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

		protected override void WriteActionXml(XmlTextWriter xmlWriter)
		{
			ActionModels.WriteXmlNodes(xmlWriter);
		}

		#endregion //Methods
	}
}
