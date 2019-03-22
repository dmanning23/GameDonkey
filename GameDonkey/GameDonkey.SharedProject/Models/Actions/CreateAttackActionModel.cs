using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class CreateAttackActionModel : BaseActionModel, ITimedActionModel, IDirectionalActionModel, IHasStateActionsListModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateAttack;
			}
		}

		public string BoneName { get; set; }
		public float Damage { get; set; }
		public TimedActionModel TimeDelta { get; set; }
		public DirectionActionModel Direction { get; set; }

		public StateActionsListModel ActionModels { get; set; }

		#endregion //Properties

		#region Initialization

		public CreateAttackActionModel()
		{
			Direction = new DirectionActionModel();
			TimeDelta = new TimedActionModel();
			ActionModels = new StateActionsListModel();
		}

		public CreateAttackActionModel(CreateAttackAction action) : base(action)
		{
			BoneName = action.BoneName;
			Damage = action.Damage;
			TimeDelta = new TimedActionModel(action);
			Direction = new DirectionActionModel(action.ActionDirection);
			ActionModels = new StateActionsListModel(action.Actions);
		}

		public CreateAttackActionModel(BaseAction action) : this(action as CreateAttackAction)
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
				case "bonename":
					{
						BoneName = value;
					}
					break;
				case "damage":
					{
						Damage = Convert.ToSingle(value);
					}
					break;
				case "timedelta":
					{
						TimeDelta.ParseXmlNode(node);
					}
					break;
				case "direction":
					{
						XmlFileBuddy.ReadChildNodes(node, Direction.ParseXmlNode);
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
			xmlWriter.WriteAttributeString("BoneName", BoneName);
			xmlWriter.WriteAttributeString("Damage", Damage.ToString());
			TimeDelta.WriteXmlNodes(xmlWriter);
			Direction.WriteXmlNodes(xmlWriter);

			ActionModels.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
