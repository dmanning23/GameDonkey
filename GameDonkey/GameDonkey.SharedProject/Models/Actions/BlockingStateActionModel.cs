using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class BlockingStateActionModel : BaseActionModel, ITimedActionModel, IHasSuccessActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.BlockState;
			}
		}

		public string BoneName { get; private set; }
		public TimedActionModel TimeDelta { get; private set; }
		public List<BaseActionModel> SuccessActions { get; private set; }

		#endregion //Properties

		#region Initialization

		public BlockingStateActionModel()
		{
			SuccessActions = new List<BaseActionModel>();
			TimeDelta = new TimedActionModel();
		}

		public BlockingStateActionModel(BlockingStateAction action) : base(action)
		{
			BoneName = action.BoneName;
			TimeDelta = new TimedActionModel(action);
			SuccessActions = new List<BaseActionModel>();
			foreach (var stateAction in action.SuccessActions)
			{
				SuccessActions.Add(StateActionFactory.CreateActionModel(stateAction));
			}
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

			if (!TimeDelta.Compare(stateAction.TimeDelta))
			{
				return false;
			}

			if (SuccessActions.Count != stateAction.SuccessActions.Count)
			{
				return false;
			}

			for (int i = 0; i < SuccessActions.Count; i++)
			{
				if (!SuccessActions[i].Compare(stateAction.SuccessActions[i]))
				{
					return false;
				}
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
				case "TimeDelta":
					{
						TimeDelta.ParseXmlNode(node);
					}
					break;
				case "SuccessActions":
					{
						var stateActions = new StateActionsModel();
						XmlFileBuddy.ReadChildNodes(node, stateActions.ParseStateAction);
						SuccessActions = stateActions.StateActions;
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
			TimeDelta.WriteXmlNodes(xmlWriter);

			xmlWriter.WriteStartElement("SuccessActions");
			foreach (var stateAction in SuccessActions)
			{
				stateAction.WriteXmlNodes(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		}

#endif

		#endregion //Methods
	}
}
