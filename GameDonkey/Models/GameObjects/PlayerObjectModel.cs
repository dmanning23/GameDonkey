using FilenameBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class PlayerObjectModel : BaseObjectModel
    {
        #region Properties

        public Filename Portrait { get; set; }

        public Filename MoveList { get; set; }

        public float BufferedInputExpire { get; set; } = 0.0f;
        public float QueuedInputExpire { get; set; } = 0.05f;

        #endregion //Properties

        #region Methods
        public PlayerObjectModel(Filename filename) : base("playerObject", filename)
        {
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
                        Portrait = new Filename();
                        Portrait.SetRelFilename(value);
                    }
                    break;
                case "portrait1":
                    {
                        Portrait = new Filename();
                        Portrait.SetFilenameRelativeToPath(Filename, value);
                    }
                    break;
                case "movelist":
                    {
                        MoveList = new Filename();
                        MoveList.SetRelFilename(value);
                    }
                    break;
                case "movelist1":
                    {
                        MoveList = new Filename();
                        MoveList.SetFilenameRelativeToPath(Filename, value);
                    }
                    break;
                case "BufferedInputExpire":
                    {
                        BufferedInputExpire = Convert.ToSingle(value);
                    }
                    break;
                case "QueuedInputExpire":
                    {
                        QueuedInputExpire = Convert.ToSingle(value);
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
