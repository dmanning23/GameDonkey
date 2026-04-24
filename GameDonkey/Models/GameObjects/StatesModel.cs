using FilenameBuddy;
using System.Xml;
using XmlBuddy;

namespace GameDonkeyLib
{
    public class StatesModel : XmlObject
    {
        #region Properties

        public Filename StateMachineFilename { get; set; }

        public Filename StateContainerFilename { get; set; }

        BaseObjectModel BaseObject { get; set; }

        #endregion //Properties

        #region Methods
        public StatesModel(BaseObjectModel baseObject)
        {
            StateMachineFilename = new Filename();
            StateContainerFilename = new Filename();
            BaseObject = baseObject;
        }

        #endregion //Methods

        #region File IO

        public override void ParseXmlNode(XmlNode node)
        {
            //what is in this node?
            var name = node.Name;
            var value = node.InnerText;

            switch (name)
            {
                case "stateMachine":
                    {
                        StateMachineFilename.SetFilenameRelativeToPath(BaseObject.Filename, value);
                    }
                    break;
                case "stateContainer":
                    {
                        StateContainerFilename.SetFilenameRelativeToPath(BaseObject.Filename, value);
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
            //write out the item tag
            xmlWriter.WriteStartElement("states");
            xmlWriter.WriteAttributeString("stateMachine", StateMachineFilename.GetRelFilename());
            xmlWriter.WriteAttributeString("stateContainer", StateContainerFilename.GetRelFilename());
            xmlWriter.WriteEndElement();
        }

        #endregion //File IO
    }
}
