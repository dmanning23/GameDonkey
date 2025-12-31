using FilenameBuddy;
using System.Collections.Generic;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
    public class SingleStateContainerModel : XmlFileBuddy
    {
        #region Properties

        public List<SingleStateActionsModel> StatesActions { get; private set; }

        #endregion //Properties

        #region Initialization

        public SingleStateContainerModel(Filename filename) : base("SingleStateContainer", filename)
        {
            StatesActions = new List<SingleStateActionsModel>();
        }

        public SingleStateContainerModel(Filename filename, StateContainer stateContainer) : this(filename)
        {
            foreach (var stateActions in stateContainer.Actions.Actions)
            {
                StatesActions.Add(new SingleStateActionsModel(stateActions.Value));
            }
        }

        public SingleStateContainerModel(Filename filename, StateMachineActions stateActions) : this(filename)
        {
            foreach (var singleStateActions in stateActions.Actions)
            {
                StatesActions.Add(new SingleStateActionsModel(singleStateActions.Value));
            }
        }

        #endregion //Initialization

        #region Methods

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
                case "states":
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
            var stateActions = new SingleStateActionsModel();
            XmlFileBuddy.ReadChildNodes(node, stateActions.ParseXmlNode);
            StatesActions.Add(stateActions);
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("States");

            foreach (var stateActions in StatesActions)
            {
                stateActions.WriteXmlNodes(xmlWriter);
            }

            xmlWriter.WriteEndElement();
        }

        #endregion //Methods
    }
}
