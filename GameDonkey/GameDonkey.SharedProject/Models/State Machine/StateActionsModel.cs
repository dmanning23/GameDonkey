using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class StateActionsModel : XmlObject
	{
		#region Properties

		public string StateName { get; private set; }
		public List<BaseActionModel> StateActions { get; private set; }

		#endregion //Properties

		#region Initialization

		public StateActionsModel()
		{
			StateActions = new List<BaseActionModel>();
		}

		public StateActionsModel(StateActions stateActions) : this()
		{
			StateName = stateActions.StateName;
			foreach (var stateAction in stateActions.Actions)
			{
				StateActions.Add(StateActionFactory.CreateActionModel(stateAction));
			}
		}

		#endregion //Initialization

		#region Methods

		
		public bool Compare(StateActionsModel inst)
		{
			if (StateName != inst.StateName)
			{
				return false;
			}

			if (StateActions.Count != inst.StateActions.Count)
			{
				return false;
			}

			for (int i = 0; i < StateActions.Count; i++)
			{
				if (!StateActions[i].Compare(inst.StateActions[i]))
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
				case "Asset":
					{
						//skip these old ass nodes
						XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
					}
					break;
				case "Type":
					{
						//Really skip these old ass nodes
					}
					break;
				case "name":
				case "StateName":
					{
						StateName = value;
					}
					break;
				case "actions":
				case "Actions":
					{
						XmlFileBuddy.ReadChildNodes(node, ParseStateAction);
					}
					break;
				default:
					{
						base.ParseXmlNode(node);
					}
					break;
			}
		}

		public void ParseStateAction(XmlNode node)
		{
			//what is the type of action?
			var actionType = node.Name;

			if ("Item" == actionType)
			{
				//legacy shit
				var typeNode = node.FirstChild;
				actionType = typeNode.InnerXml;
			}

			//create the correct action
			var stateAction = StateActionFactory.CreateActionModel(actionType);
			XmlFileBuddy.ReadChildNodes(node, stateAction.ParseXmlNode);
			StateActions.Add(stateAction);
		}

#if !WINDOWS_UWP

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("State");

			xmlWriter.WriteAttributeString("StateName", StateName);

			xmlWriter.WriteStartElement("Actions");
			foreach (var stateAction in StateActions)
			{
				stateAction.WriteXmlNodes(xmlWriter);
			}
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
		}

#endif

		#endregion //Methods
	}
}
