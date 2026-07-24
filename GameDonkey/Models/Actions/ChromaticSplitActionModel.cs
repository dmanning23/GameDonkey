using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ChromaticSplitActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ChromaticSplit;
            }
        }

        public float MaxDistance { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticSplitActionModel()
        {
            MaxDistance = 0.05f;
            Duration = 0.3f;
        }

        public ChromaticSplitActionModel(ChromaticSplitAction action) : base(action)
        {
            MaxDistance = action.MaxDistance;
            Duration = action.Duration;
        }

        public ChromaticSplitActionModel(BaseAction action) : this(action as ChromaticSplitAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "maxdistance":
                    {
                        MaxDistance = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("MaxDistance", MaxDistance.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
