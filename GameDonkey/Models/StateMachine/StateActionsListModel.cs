using System;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
    public class StateActionsListModel : XmlObject
    {
        #region Properties

        public List<BaseActionModel> ActionModels { get; private set; }

        #endregion //Properties

        #region Initialization

        public StateActionsListModel()
        {
            ActionModels = new List<BaseActionModel>();
        }

        public StateActionsListModel(List<BaseAction> actions) : this()
        {
            foreach (var stateAction in actions)
            {
                ActionModels.Add(StateActionFactory.CreateActionModel(stateAction));
            }
        }

        public StateActionsListModel(StateActionsList actions) : this(actions.Actions)
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
                        XmlFileBuddy.ReadChildNodes(node, ParseStateAction);
                    }
                    break;
                default:
                    {
                        NodeError(node);
                    }
                    break;
            }
        }

        public void ParseStateAction(XmlNode node)
        {
            try
            {
                //what is the type of action?
                var actionType = node.Name;

                //create the correct action
                var stateAction = StateActionFactory.CreateActionModel(actionType);
                XmlFileBuddy.ReadChildNodes(node, stateAction.ParseXmlNode);
                ActionModels.Add(stateAction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {node.Name}", ex);
            }
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("actions");
            foreach (var stateAction in ActionModels)
            {
                stateAction.WriteXmlNodes(xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        #endregion //Methods
    }
}
