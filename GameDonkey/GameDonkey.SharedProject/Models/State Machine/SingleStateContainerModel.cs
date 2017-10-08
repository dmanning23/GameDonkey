using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
	public class SingleStateContainerModel : XmlFileBuddy
	{
		#region Properties

		public List<StateActionsModel> StatesActions = new List<StateActionsModel>();

		#endregion //Properties

		#region Methods

		public SingleStateContainerModel(Filename filename) : base("SingleStateContainer", filename)
		{
			StatesActions = new List<StateActionsModel>();
		}

		public bool Compare(SingleStateContainerModel inst)
		{
			if (StatesActions.Count != inst.StatesActions.Count)
			{
				return false;
			}

			for (int i = 0; i < StatesActions.Count; i++)
			{
				if (!StatesActions[i].Compare(inst.StatesActions[i]))
				{
					return false;
				}
			}

			return Filename == inst.Filename;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			//what is in this node?
			var name = node.Name;
			var value = node.InnerText;

			switch (name)
			{
				case "States":
					{
						XmlFileBuddy.ReadChildNodes(node, ParseStates);
					}
					break;
				default:
					{
						NodeError(node);
					}
					break;
			}
		}

		private void ParseStates(XmlNode node)
		{
			var stateActions = new StateActionsModel();
			XmlFileBuddy.ReadChildNodes(node, stateActions.ParseXmlNode);
			StatesActions.Add(stateActions);
		}

#if !WINDOWS_UWP

		public override void WriteXmlNodes(XmlTextWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("States");

			foreach (var stateActions in StatesActions)
			{
				stateActions.WriteXmlNodes(xmlWriter);
			}

			xmlWriter.WriteEndElement();
		}

#endif

		#endregion //Methods
	}
}
