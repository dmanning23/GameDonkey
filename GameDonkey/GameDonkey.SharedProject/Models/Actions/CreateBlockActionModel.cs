using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class CreateBlockActionModel : BaseActionModel, ITimedActionModel, IHasSuccessActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateBlock;
			}
		}

		public TimedActionModel TimeDelta { get; set; }
		public List<BaseActionModel> SuccessActions { get; private set; }

		#endregion //Properties

		#region Initialization

		public CreateBlockActionModel()
		{
			SuccessActions = new List<BaseActionModel>();
			TimeDelta = new TimedActionModel();
		}

		public CreateBlockActionModel(CreateBlockAction action) : base(action)
		{
			TimeDelta = new TimedActionModel(action);
			SuccessActions = new List<BaseActionModel>();
			foreach (var stateAction in action.SuccessActions)
			{
				SuccessActions.Add(StateActionFactory.CreateActionModel(stateAction));
			}
		}

		public CreateBlockActionModel(BaseAction action) : this(action as CreateBlockAction)
		{
		}

		#endregion //Initialization

		#region Methods

		public override bool Compare(BaseActionModel inst)
		{
			var stateAction = inst as CreateBlockActionModel;
			if (null == stateAction)
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
							SuccessActions.Add(new PlaySoundActionModel()
							{
								Filename = new Filename(value)
							});
						}
					}
					break;
				case "successactions":
					{
						var stateActions = new StateActionsModel();
						XmlFileBuddy.ReadChildNodes(node, stateActions.ParseStateAction);
						SuccessActions.AddRange(stateActions.StateActions);
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
