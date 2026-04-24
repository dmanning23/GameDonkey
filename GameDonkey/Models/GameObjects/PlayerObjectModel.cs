using FilenameBuddy;
using System.Xml;

namespace GameDonkeyLib
{
    public class PlayerObjectModel : BaseObjectModel
    {
        #region Properties

        public Filename Portrait { get; set; }

        #endregion //Properties

        #region Methods
        public PlayerObjectModel(Filename filename) : base("playerObject", filename)
        {
            Portrait = new Filename();
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
                case "portrait":
                    {
                        Portrait.SetRelFilename(value);
                    }
                    break;
                case "portrait1":
                    {
                        Portrait.SetFilenameRelativeToPath(Filename, value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        #endregion //File IO
    }
}
