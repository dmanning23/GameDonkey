using FilenameBuddy;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
    public class TemplateContainerModel : XmlFileBuddy
    {
        #region Properties

        public StateActionsListModel StatesActions { get; private set; }

        #endregion //Properties

        #region Initialization

        public TemplateContainerModel(Filename filename) : base("templateContainer", filename)
        {
            StatesActions = new StateActionsListModel();
        }

        public TemplateContainerModel(Filename filename, StateActionsList stateActions) : this(filename)
        {
            StatesActions = new StateActionsListModel(stateActions);
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
                case "actions":
                    {
                        StatesActions.ParseXmlNode(node);
                    }
                    break;
                default:
                    {
                        NodeError(node);
                    }
                    break;
            }
        }

        public override void WriteXmlNodes(XmlTextWriter xmlWriter)
        {
            StatesActions.WriteXmlNodes(xmlWriter);
        }

        #endregion //Methods
    }
}
