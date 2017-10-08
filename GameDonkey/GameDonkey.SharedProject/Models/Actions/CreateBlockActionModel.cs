using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class CreateBlockActionModel : BaseActionModel, ITimedActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateBlock;
			}
		}

		public TimedActionModel TimeDelta { get; private set; }

		#endregion //Properties

		#region Methods

		public CreateBlockActionModel()
		{
			TimeDelta = new TimedActionModel();
		}

		public override bool Compare(BaseActionModel inst)
		{
			if (!base.Compare(inst))
			{
				return false;
			}

			var stateAction = inst as CreateBlockActionModel;
			if (null == stateAction)
			{
				return false;
			}

			if (!TimeDelta.Compare(stateAction.TimeDelta))
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
			TimeDelta.WriteXmlNodes(xmlWriter);
		}

#endif

		#endregion //Methods
	}
}
