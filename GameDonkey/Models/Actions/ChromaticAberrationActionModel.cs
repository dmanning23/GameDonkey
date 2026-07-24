using ScreenFXBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ChromaticAberrationActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ChromaticAberration;
            }
        }

        public float Distance { get; set; }
        public float Duration { get; set; }
        public FadeCurve Curve { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticAberrationActionModel()
        {
            Distance = 0.1f;
            Duration = 2f;
            Curve = FadeCurve.Linear;
        }

        public ChromaticAberrationActionModel(ChromaticAberrationAction action) : base(action)
        {
            Distance = action.Distance;
            Duration = action.Duration;
            Curve = action.Curve;
        }

        public ChromaticAberrationActionModel(BaseAction action) : this(action as ChromaticAberrationAction)
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
                case "distance":
                    {
                        Distance = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                case "curve":
                    {
                        Curve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
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
            xmlWriter.WriteAttributeString("Distance", Distance.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
            xmlWriter.WriteAttributeString("Curve", Curve.ToString());
        }

        #endregion //Methods
    }
}
