using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ScreenTiltActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ScreenTilt;
            }
        }

        public float Angle { get; set; }
        public float Duration { get; set; }
        public float Delta { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenTiltActionModel()
        {
            Angle = 3.0f;
            Duration = 0.4f;
            Delta = 0.2f;
        }

        public ScreenTiltActionModel(ScreenTiltAction action) : base(action)
        {
            Angle = action.Angle;
            Duration = action.Duration;
            Delta = action.Delta;
        }

        public ScreenTiltActionModel(BaseAction action) : this(action as ScreenTiltAction)
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
                case "angle":
                    {
                        Angle = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                case "delta":
                    {
                        Delta = Convert.ToSingle(value);
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
            xmlWriter.WriteAttributeString("Angle", Angle.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
            xmlWriter.WriteAttributeString("Delta", Delta.ToString());
        }

        #endregion //Methods
    }
}
