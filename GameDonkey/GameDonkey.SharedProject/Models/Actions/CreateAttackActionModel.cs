using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class CreateAttackActionModel : BaseActionModel, ITimedActionModel, IDirectionalActionModel, IHasSuccessActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateAttack;
			}
		}

		public string BoneName { get; private set; }
		public float Damage { get; private set; }
		public TimedActionModel TimeDelta { get; private set; }
		public DirectionActionModel Direction { get; private set; }
		public List<BaseActionModel> SuccessActions { get; private set; }

		#endregion //Properties

		#region Methods

		public CreateAttackActionModel()
		{
			Direction = new DirectionActionModel();
			TimeDelta = new TimedActionModel();
			SuccessActions = new List<BaseActionModel>();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as CreateAttackActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (BoneName != stateAction.BoneName)
			{
				return false;
			}

			if (!Damage.AlmostEqual(stateAction.Damage))
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

			return true;
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
				case "Damage":
					{
						Damage = Convert.ToSingle(value);
					}
					break;
				case "TimeDelta":
					{
						TimeDelta.ParseXmlNode(node);
					}
					break;
				case "Direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
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
			xmlWriter.WriteAttributeString("Damage", Damage.ToString());
			TimeDelta.WriteXmlNodes(xmlWriter);
			Direction.WriteXmlNodes(xmlWriter);

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
